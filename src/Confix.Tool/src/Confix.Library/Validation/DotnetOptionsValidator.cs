using System.Diagnostics;
using Confix.Tool.Commands.Logging;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Validation;

/// <summary>Executes the application's option contracts using its runtime and dependencies.</summary>
internal sealed class DotnetOptionsValidator : IConfigurationValidator
{
    public async Task<JsonNode?> ValidateAsync(IMiddlewareContext context, ValidationConfiguration settings, bool export)
    {
        if (context.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore) && noRestore)
            throw new ExitException("--no-restore is not supported for dotnet-options validation.");
        var directory = context.Features.Get<ConfigurationFeature>().EnsureProject().Directory!.FullName;
        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        var projects = Directory.GetFiles(directory, "*.csproj");
        if (projects.Length != 1) throw new ExitException("Code-first validation requires exactly one project file in the project directory.");
        context.Parameter.TryGet(DotnetConfigurationOptions.Instance, out string? configuration);
        configuration = string.IsNullOrWhiteSpace(configuration) ? "Debug" : configuration;
        var args = new List<string> { "build", projects[0], "--configuration", configuration, "--nologo", "-v:q", "--disable-build-servers" };
        if (settings.Framework is { } framework) args.AddRange(["--framework", framework]);
        context.Logger.Information("Building code-first validation contracts.");
        var build = await RunAsync(args, null, directory, context.CancellationToken);
        if (build.ExitCode != 0) throw new ExitException("Code-first project build failed. Run dotnet build on the project for compiler diagnostics.");
        context.Logger.Debug("Code-first project build completed.");
        var properties = new List<string> { "msbuild", projects[0], "-nologo", "-nodeReuse:false", "-getProperty:TargetPath,TargetFramework,TargetFrameworks", "-property:Configuration=" + configuration };
        if (settings.Framework is { } tfm) properties.Add("-property:TargetFramework=" + tfm);
        var result = await RunAsync(properties, null, directory, context.CancellationToken);
        if (result.ExitCode != 0) throw new ExitException("Could not determine the code-first target output.");
        var evaluated = JsonNode.Parse(result.Output)?["Properties"];
        if (string.IsNullOrEmpty(evaluated?["TargetFramework"]?.GetValue<string>()))
            throw new ExitException("Set project.validation.framework for a multi-target project.");
        var target = evaluated!["TargetPath"]!.GetValue<string>();
        var runtimeConfig = Path.ChangeExtension(target, ".runtimeconfig.json");
        var deps = Path.ChangeExtension(target, ".deps.json");
        if (!File.Exists(runtimeConfig)) throw new ExitException("Code-first validation requires a host project with a runtimeconfig.json.");
        context.Logger.Debug("Code-first target selected: " + target);
        var runner = Path.Combine(AppContext.BaseDirectory, "Confix.Runner.dll");
        if (!File.Exists(runner)) throw new ExitException("The Confix installation is missing its validation runner.");
        JsonNode? schema = null;
        foreach (var file in files)
        {
            var request = new JsonObject { ["version"] = 1, ["configuration"] = file.Content?.DeepClone(),
                ["coverage"] = settings.Coverage ?? "strict", ["exportSchema"] = export };
            var validation = await RunAsync(["exec", "--runtimeconfig", runtimeConfig, "--depsfile", deps, runner, target],
                request.ToJsonString(), directory, context.CancellationToken);
            JsonNode? response;
            try { response = JsonNode.Parse(validation.Output); }
            catch { throw new ExitException("The code-first runner returned an invalid response."); }
            if (response?["version"]?.GetValue<int>() != 1) throw new ExitException("Unsupported code-first runner protocol.");
            var errors = response["errors"]?.AsArray() ?? throw new ExitException("Invalid code-first validation response.");
            if (validation.ExitCode != 0 || errors.Count > 0)
                throw new ExitException("Code-first validation failed for " + file.InputFile.Name + ":\n" +
                    string.Join('\n', errors.Select(e => Spectre.Console.Markup.Escape(e!.GetValue<string>()))));
            schema = response["schema"]?.DeepClone();
        }
        return schema;
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(IEnumerable<string> arguments, string? input,
        string directory, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        var info = new ProcessStartInfo("dotnet") { WorkingDirectory = directory, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = Process.Start(info) ?? throw new ExitException("Could not start dotnet.");
        using var registration = timeout.Token.Register(() => { try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { } });
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        if (input is not null) await process.StandardInput.WriteAsync(input.AsMemory(), timeout.Token);
        process.StandardInput.Close();
        await process.WaitForExitAsync(timeout.Token);
        await stderr;
        return (process.ExitCode, await stdout);
    }
}
