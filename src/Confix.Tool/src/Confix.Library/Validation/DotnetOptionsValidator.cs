using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Spectre.Console;

namespace Confix.Tool.Validation;

/// <summary>
/// Executes the application's option contracts using its runtime and dependencies.
/// </summary>
internal sealed class DotnetOptionsValidator : IConfigurationValidator
{
    private const int ProtocolVersion = 1;
    private const string RunnerAssembly = "Confix.Runner.dll";
    private const string DefaultConfiguration = "Debug";

    public async Task<JsonNode?> ValidateAsync(
        IMiddlewareContext context,
        ValidationConfiguration settings,
        bool exportSchema)
    {
        if (context.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore) && noRestore)
        {
            throw new ExitException("--no-restore is not supported for dotnet-options validation.");
        }

        var runner = Path.Combine(AppContext.BaseDirectory, RunnerAssembly);

        if (!File.Exists(runner))
        {
            throw new ExitException("The Confix installation is missing its validation runner.");
        }

        var directory = context.Features
            .Get<ConfigurationFeature>()
            .EnsureProject()
            .Directory!
            .FullName;

        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        var target = await BuildTargetAsync(context, directory, settings.Framework);

        JsonNode? schema = null;

        foreach (var file in files)
        {
            // Contracts are identical for every input, so the schema is requested only once.
            var request = new JsonObject
            {
                ["version"] = ProtocolVersion,
                ["configuration"] = file.Content?.DeepClone(),
                ["coverage"] = settings.Coverage ?? ValidationConfiguration.Strict,
                ["exportSchema"] = exportSchema && schema is null
            };

            string[] arguments =
            [
                "exec",
                "--runtimeconfig", target.RuntimeConfig,
                "--depsfile", target.Dependencies,
                runner,
                target.Assembly
            ];

            var validation = await DotnetValidationProcess.RunAsync(
                arguments,
                request.ToJsonString(),
                directory,
                context.CancellationToken);

            schema = ReadResponse(validation.Output, validation.ExitCode, file.InputFile.Name)
                ?? schema;
        }

        return schema;
    }

    private static async Task<ValidationTarget> BuildTargetAsync(
        IMiddlewareContext context,
        string directory,
        string? framework)
    {
        var projects = Directory.GetFiles(directory, "*.csproj");

        if (projects.Length != 1)
        {
            throw new ExitException(
                "Code-first validation requires exactly one project file in the project directory.");
        }

        context.Parameter.TryGet(
            DotnetConfigurationOptions.Instance,
            out string? configuration);

        if (string.IsNullOrWhiteSpace(configuration))
        {
            configuration = DefaultConfiguration;
        }

        await BuildAsync(context, directory, projects[0], configuration, framework);

        return await ResolveTargetAsync(context, directory, projects[0], configuration, framework);
    }

    private static async Task BuildAsync(
        IMiddlewareContext context,
        string directory,
        string project,
        string configuration,
        string? framework)
    {
        var arguments = new List<string>
        {
            "build",
            project,
            "--configuration", configuration,
            "--nologo",
            "-v:q",
            "--disable-build-servers"
        };

        if (framework is not null)
        {
            arguments.AddRange(["--framework", framework]);
        }

        context.Logger.Information("Building code-first validation contracts.");

        var build = await DotnetValidationProcess.RunAsync(
            arguments,
            null,
            directory,
            context.CancellationToken);

        if (build.ExitCode != 0)
        {
            throw new ExitException(
                "Code-first project build failed. " +
                "Run dotnet build on the project for compiler diagnostics.");
        }

        context.Logger.Debug("Code-first project build completed.");
    }

    private static async Task<ValidationTarget> ResolveTargetAsync(
        IMiddlewareContext context,
        string directory,
        string project,
        string configuration,
        string? framework)
    {
        var arguments = new List<string>
        {
            "msbuild",
            project,
            "-nologo",
            "-nodeReuse:false",
            "-getProperty:TargetPath,TargetFramework,TargetFrameworks",
            "-property:Configuration=" + configuration
        };

        if (framework is not null)
        {
            arguments.Add("-property:TargetFramework=" + framework);
        }

        var result = await DotnetValidationProcess.RunAsync(
            arguments,
            null,
            directory,
            context.CancellationToken);

        if (result.ExitCode != 0)
        {
            throw new ExitException("Could not determine the code-first target output.");
        }

        var properties = JsonNode.Parse(result.Output)?["Properties"];

        if (string.IsNullOrEmpty(properties?["TargetFramework"]?.GetValue<string>()))
        {
            throw new ExitException("Set project.validation.framework for a multi-target project.");
        }

        var assembly = properties["TargetPath"]!.GetValue<string>();
        var runtimeConfig = Path.ChangeExtension(assembly, ".runtimeconfig.json");
        var dependencies = Path.ChangeExtension(assembly, ".deps.json");

        if (!File.Exists(runtimeConfig) || !File.Exists(dependencies))
        {
            throw new ExitException(
                "Code-first validation requires a host project that produces " +
                "runtimeconfig.json and deps.json.");
        }

        context.Logger.Debug("Code-first target selected: " + assembly);

        return new ValidationTarget(assembly, runtimeConfig, dependencies);
    }

    private static JsonNode? ReadResponse(string output, int exitCode, string inputFileName)
    {
        JsonNode? response;

        try
        {
            response = JsonNode.Parse(output);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ExitException("The code-first runner returned an invalid response.");
        }

        if (response?["version"]?.GetValue<int>() != ProtocolVersion)
        {
            throw new ExitException("Unsupported code-first runner protocol.");
        }

        var errors = response["errors"]?.AsArray()
            ?? throw new ExitException("Invalid code-first validation response.");

        if (exitCode != 0 || errors.Count > 0)
        {
            var messages = errors.Select(error => Markup.Escape(error!.GetValue<string>()));

            throw new ExitException(
                $"Code-first validation failed for {inputFileName}:\n" +
                string.Join('\n', messages));
        }

        return response["schema"]?.DeepClone();
    }

    private sealed record ValidationTarget(
        string Assembly,
        string RuntimeConfig,
        string Dependencies);
}
