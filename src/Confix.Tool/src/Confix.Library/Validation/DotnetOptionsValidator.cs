using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Validation;

/// <summary>Executes the application's option contracts using its runtime and dependencies.</summary>
internal sealed class DotnetOptionsValidator : IConfigurationValidator
{
    public async Task<JsonNode?> ValidateAsync(IMiddlewareContext context, ValidationConfiguration settings, bool exportSchema)
    {
        if (context.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore) && noRestore)
        {
            throw new ExitException("--no-restore is not supported for dotnet-options validation.");
        }

        var directory = context.Features.Get<ConfigurationFeature>().EnsureProject().Directory!.FullName;
        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        var target = await BuildTargetAsync(context, directory, settings.Framework);
        var runner = Path.Combine(AppContext.BaseDirectory, "Confix.Runner.dll");
        if (!File.Exists(runner))
        {
            throw new ExitException("The Confix installation is missing its validation runner.");
        }

        JsonNode? schema = null;
        foreach (var file in files)
        {
            // Contracts are identical for every input, so the schema is requested only once.
            var requestSchema = exportSchema && schema is null;
            var request = new JsonObject
            {
                ["version"] = 1,
                ["configuration"] = file.Content?.DeepClone(),
                ["coverage"] = settings.Coverage ?? "strict",
                ["exportSchema"] = requestSchema
            };
            var validation = await DotnetValidationProcess.RunAsync(["exec", "--runtimeconfig", target.RuntimeConfig, "--depsfile", target.Dependencies, runner, target.Assembly],
                request.ToJsonString(), directory, context.CancellationToken);
            schema = ReadResponse(validation.Output, validation.ExitCode, file.InputFile.Name) ?? schema;
        }
        return schema;
    }

    private static async Task<ValidationTarget> BuildTargetAsync(
        IMiddlewareContext context, string directory, string? framework)
    {
        var projects = Directory.GetFiles(directory, "*.csproj");
        if (projects.Length != 1)
        {
            throw new ExitException("Code-first validation requires exactly one project file in the project directory.");
        }
        context.Parameter.TryGet(DotnetConfigurationOptions.Instance, out string? configuration);
        configuration = string.IsNullOrWhiteSpace(configuration) ? "Debug" : configuration;
        var buildArguments = new List<string>
        {
            "build", projects[0], "--configuration", configuration,
            "--nologo", "-v:q", "--disable-build-servers"
        };
        if (framework is not null)
        {
            buildArguments.AddRange(["--framework", framework]);
        }
        context.Logger.Information("Building code-first validation contracts.");
        var build = await DotnetValidationProcess.RunAsync(buildArguments, null, directory, context.CancellationToken);
        if (build.ExitCode != 0)
        {
            throw new ExitException("Code-first project build failed. Run dotnet build on the project for compiler diagnostics.");
        }
        context.Logger.Debug("Code-first project build completed.");
        var propertyArguments = new List<string>
        {
            "msbuild", projects[0], "-nologo", "-nodeReuse:false",
            "-getProperty:TargetPath,TargetFramework,TargetFrameworks",
            "-property:Configuration=" + configuration
        };
        if (framework is not null)
        {
            propertyArguments.Add("-property:TargetFramework=" + framework);
        }
        var result = await DotnetValidationProcess.RunAsync(propertyArguments, null, directory, context.CancellationToken);
        if (result.ExitCode != 0)
        {
            throw new ExitException("Could not determine the code-first target output.");
        }
        var evaluated = JsonNode.Parse(result.Output)?["Properties"];
        if (string.IsNullOrEmpty(evaluated?["TargetFramework"]?.GetValue<string>()))
        {
            throw new ExitException("Set project.validation.framework for a multi-target project.");
        }
        var target = evaluated!["TargetPath"]!.GetValue<string>();
        var runtimeConfig = Path.ChangeExtension(target, ".runtimeconfig.json");
        var deps = Path.ChangeExtension(target, ".deps.json");
        if (!File.Exists(runtimeConfig) || !File.Exists(deps))
        {
            throw new ExitException("Code-first validation requires a host project that produces runtimeconfig.json and deps.json.");
        }
        context.Logger.Debug("Code-first target selected: " + target);
        return new ValidationTarget(target, runtimeConfig, deps);
    }

    private static JsonNode? ReadResponse(string output, int exitCode, string inputFileName)
    {
        JsonNode? response;
        try
        {
            response = JsonNode.Parse(output);
        }
        catch
        {
            throw new ExitException("The code-first runner returned an invalid response.");
        }

        if (response?["version"]?.GetValue<int>() != 1)
        {
            throw new ExitException("Unsupported code-first runner protocol.");
        }
        var errors = response["errors"]?.AsArray()
            ?? throw new ExitException("Invalid code-first validation response.");
        if (exitCode != 0 || errors.Count > 0)
        {
            var messages = errors.Select(error => Spectre.Console.Markup.Escape(error!.GetValue<string>()));
            throw new ExitException("Code-first validation failed for " + inputFileName + ":\n" +
                string.Join('\n', messages));
        }
        return response["schema"]?.DeepClone();
    }

    private sealed record ValidationTarget(string Assembly, string RuntimeConfig, string Dependencies);
}
