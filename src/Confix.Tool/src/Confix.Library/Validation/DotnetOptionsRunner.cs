using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Validation;

/// <summary>Builds the code-first host project and exchanges requests with its runner.</summary>
internal static class DotnetOptionsRunner
{
    public const int ProtocolVersion = 1;
    private const string RunnerAssembly = "Confix.Runner.dll";
    private const string DefaultConfiguration = "Debug";

    public static string ResolveRunner()
    {
        var runner = Path.Combine(AppContext.BaseDirectory, RunnerAssembly);

        if (!File.Exists(runner))
        {
            throw new ExitException("The Confix installation is missing its validation runner.");
        }

        return runner;
    }

    /// <summary>
    /// Builds and resolves the target once per pipeline run; schema derivation and validation
    /// share the same build.
    /// </summary>
    public static async Task<ValidationTarget> EnsureTargetAsync(
        IMiddlewareContext context,
        string directory,
        string? framework)
    {
        if (context.Features.TryGet(out DotnetValidationTargetFeature? cached))
        {
            return cached.Target;
        }

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

        var target = await ResolveTargetAsync(
            context, directory, projects[0], configuration, framework);

        context.Features.Set(new DotnetValidationTargetFeature(target));

        return target;
    }

    public static async Task<RunnerResponse> InvokeAsync(
        IMiddlewareContext context,
        ValidationTarget target,
        string directory,
        JsonObject request)
    {
        string[] arguments =
        [
            "exec",
            "--runtimeconfig", target.RuntimeConfig,
            "--depsfile", target.Dependencies,
            ResolveRunner(),
            target.Assembly
        ];

        var result = await DotnetValidationProcess.RunAsync(
            arguments,
            request.ToJsonString(),
            directory,
            context.CancellationToken);

        JsonNode? response;

        try
        {
            response = JsonNode.Parse(result.Output);
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

        return new RunnerResponse(
            result.ExitCode,
            errors.Select(e => e!.GetValue<string>()).ToArray(),
            response["schema"]?.DeepClone());
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
}

internal sealed record ValidationTarget(
    string Assembly,
    string RuntimeConfig,
    string Dependencies);

internal sealed record RunnerResponse(
    int ExitCode,
    IReadOnlyList<string> Errors,
    JsonNode? Schema);

internal sealed record DotnetValidationTargetFeature(ValidationTarget Target);
