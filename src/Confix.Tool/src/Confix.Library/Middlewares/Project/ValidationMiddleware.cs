using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Schema;
using Json.Pointer;
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
        configuration.EnsureProjectScope();
        var solution = configuration.EnsureSolution();
        var project = configuration.EnsureProject();

        var loadSchema = _schemaStore.GetSchema(solution, project);

        var evaluationOptions = new EvaluationOptions
        {
            OutputFormat = Json.Schema.OutputFormat.Hierarchical
        };

        var failed = false;
        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        foreach (var file in files)
        {
            var jsonFile = await file.GetContent(cancellationToken);

            var result = loadSchema.Evaluate(
                JsonSerializer.SerializeToElement(jsonFile),
                evaluationOptions);

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

        ProcessResult(results);

        void ProcessResult(EvaluationResults root)
        {
            var path = root.InstanceLocation.GetSegments();
            if (!root.IsValid)
            {
                if (root.Errors is { Count: > 0 })
                {
                    var node = Resolve(path);
                    foreach (var (keyword, error) in root.Errors!)
                    {
                        // applicator keywords only aggregate the errors of their subschemas,
                        // which are reported by the nested results
                        if (IsApplicator(keyword))
                        {
                            continue;
                        }

                        node.AddNode(
                            $"{Glyph.Cross.ToMarkup()} {error.EscapeMarkup()}");
                    }
                }

                // `Details` is not annotated as nullable but is null for leaf results.
                IEnumerable<EvaluationResults> details =
                    (IEnumerable<EvaluationResults>?) root.Details ?? [];
                if (details.Any(x => x.EvaluationPath.IsAnyOf() && x.Details?.Count > 0))
                {
                    details = details
                        .Where(x => !x.EvaluationPath.IsAnyOf() || x.Details?.Count > 0)
                        .ToArray();
                }

                foreach (var result in details)
                {
                    ProcessResult(result);
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
                node = Resolve(path.Take(path.Length - 1).ToArray())
                    .AddNode("[yellow]" + path.Last().EscapeMarkup() + "[/]");
                lookup.Add(pathString, node);
            }

            return node;
        }

        return tree;
    }

    public static bool IsApplicator(string keyword)
        => keyword is "properties" or "anyOf" or "oneOf" or "allOf" or "items" or
            "additionalProperties" or "prefixItems" or "patternProperties";

    public static string[] GetSegments(this JsonPointer pointer)
    {
        var segments = new string[pointer.SegmentCount];
        for (var index = 0; index < segments.Length; index++)
        {
            segments[index] = pointer[index].ToString();
        }

        return segments;
    }

    public static bool IsAnyOf(this JsonPointer pointer)
    {
        if (pointer.SegmentCount < 2)
        {
            return false;
        }

        return pointer[pointer.SegmentCount - 2].ToString() == "anyOf";
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
                $"Call 'confix restore' to generate the schema.");
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
}
