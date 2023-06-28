using System.Text.Json.Nodes;
using Confix.Entities.Project.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Schema;
using Json.Schema;
using Spectre.Console;

namespace Confix.Tool.Middlewares.Project;

public sealed class ValidationMiddleware : IMiddleware
{
    private readonly ISchemaStore _schemaStore;

    public ValidationMiddleware(ISchemaStore schemaStore)
    {
        _schemaStore = schemaStore;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Validating the project files");

        var cancellationToken = context.CancellationToken;

        var configuration = context.Features.Get<ConfigurationFeature>();
        var solution = configuration.EnsureSolution();
        var project = configuration.EnsureProject();

        var loadSchema = project.GetJsonSchema() ?? _schemaStore.GetSchema(solution, project);

        var evaluationOptions = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            Log = context.Logger.CreateLog()
        };

        var failed = false;
        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        foreach (var file in files)
        {
            var jsonFile = await file.GetContent(cancellationToken);

            var result = loadSchema.Evaluate(jsonFile, evaluationOptions);

            if (!result.IsValid)
            {
                failed = true;
                context.Logger.InvalidConfigurationFile(file);

                context.Console.Write(result.ToTree());
            }
            else
            {
                context.Logger.ValidConfigurationFile(file);
            }
        }

        if (failed)
        {
            throw new ExitException("The configuration files are invalid.");
        }

        await next(context);
    }
}

file static class Log
{
    public static void InvalidConfigurationFile(
        this IConsoleLogger console,
        ConfigurationFile file)
    {
        console.Error($"The configuration file {file.InputFile.ToLink()} is invalid.");
    }

    public static void ValidConfigurationFile(
        this IConsoleLogger console,
        ConfigurationFile file)
    {
        console.Success($"The configuration file {file.InputFile.ToLink()} is valid.");
    }
}

file static class Extensions
{
    public static Tree ToTree(this EvaluationResults results)
    {
        var tree = new Tree(Text.Empty);
        var lookup = new Dictionary<string, IHasTreeNodes>();

        foreach (var result in results.Details)
        {
            var path = result.InstanceLocation.Segments.Select(x => x.Value).ToArray();
            var node = Resolve(path);
            if (result.HasErrors)
            {
                foreach (var (_, error) in result.Errors!)
                {
                    node.AddNode($"{Glyph.Cross.ToMarkup()} {error.EscapeMarkup()}");
                }
            }
        }

        IHasTreeNodes Resolve(string[] path)
        {
            if (path.Length == 0)
            {
                return tree;
            }

            var pathString = string.Join('/', path);
            if (!lookup.TryGetValue(pathString, out var node))
            {
                node = Resolve(path.Take(path.Length - 1).ToArray()).AddNode(path.Last().AsPath());
                lookup.Add(pathString, node);
            }

            return node;
        }

        return tree;
    }

    private static T AddResult<T>(this T node, EvaluationResults results) where T : IHasTreeNodes
    {
        if (results.HasErrors)
        {
            foreach (var (_, error) in results.Errors!)
            {
                node.AddNode(error.AsError());
            }
        }
        else
        {
            foreach (var result in results.Details)
            {
                var lastSegment = result.InstanceLocation.Segments.LastOrDefault()
                    ?.Value.EscapeMarkup() ?? "";
                node.AddNode(lastSegment).AddResult(result);
            }
        }

        return node;
    }

    public static JsonSchema GetSchema(
        this ISchemaStore store,
        SolutionDefinition solution,
        ProjectDefinition project)
    {
        if (!store.TryLoad(solution, project, out var schema))
        {
            throw new ExitException(
                $"The schema for the project '{project.Name}' could not be found. " +
                $"Call 'confix reload' to generate the schema.");
        }

        return schema;
    }

    public static async Task<JsonNode> GetContent(
        this ConfigurationFile file,
        CancellationToken cancellationToken)
    {
        var result = await file.TryLoadContentAsync(cancellationToken);
        return result ?? throw new ExitException(
            $"The configuration file '{file.InputFile.Name}' could not be found.");
    }

    public static CustomLogger CreateLog(this IConsoleLogger log)
    {
        return new CustomLogger(log);
    }
}

/// <summary>
/// Used to log processing details.
/// </summary>
file class CustomLogger : ILog
{
    private readonly IConsoleLogger _logger;

    public CustomLogger(IConsoleLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs a message with a newline.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="indent"></param>
    public void Write(Func<string> message, int indent = 0)
    {
        _logger.Trace(message().EscapeMarkup());
    }
}
