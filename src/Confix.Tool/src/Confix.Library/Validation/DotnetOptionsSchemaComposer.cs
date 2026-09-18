using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;
using Json.Schema;

namespace Confix.Tool.Validation;

/// <summary>
/// Derives the project schema from the application's option contracts and stores it where
/// json-schema composition stores its result, so defaults initialization, init and IDE
/// integration work identically for both providers.
/// </summary>
internal static class DotnetOptionsSchemaComposer
{
    public static async Task<FileInfo> ComposeAndStoreAsync(
        IMiddlewareContext context,
        ValidationConfiguration settings,
        ISchemaStore schemaStore)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        configuration.EnsureProjectScope();

        var project = configuration.EnsureProject();
        var solution = configuration.EnsureSolution();
        var directory = project.Directory!.FullName;

        context.SetStatus("Deriving the schema from the option contracts...");

        var target = await DotnetOptionsRunner.EnsureTargetAsync(
            context, directory, settings.Framework);

        // The current document rides along so host capture sees realistic configuration,
        // but its validation errors are irrelevant for schema derivation.
        var document = context.Features
            .Get<ConfigurationFileFeature>()
            .Files
            .FirstOrDefault()?
            .Content?
            .DeepClone() ?? new JsonObject();

        var request = new JsonObject
        {
            ["version"] = DotnetOptionsRunner.ProtocolVersion,
            ["configuration"] = document,
            ["coverage"] = settings.Coverage ?? ValidationConfiguration.Strict,
            ["exportSchema"] = true
        };

        var response = await DotnetOptionsRunner.InvokeAsync(context, target, directory, request);

        if (response.Schema is null)
        {
            throw new ExitException(
                "The code-first schema could not be derived:\n" +
                string.Join('\n', response.Errors.DefaultIfEmpty("The runner returned no schema.")));
        }

        var schema = JsonSchema.FromText(response.Schema.ToJsonString());

        return await schemaStore.StoreAsync(solution, project, schema, context.CancellationToken);
    }

    /// <summary>The validation settings that apply to the project in the current context.</summary>
    public static ValidationConfiguration? ResolveSettings(IMiddlewareContext context)
    {
        var project = context.Features.Get<ConfigurationFeature>().EnsureProject();

        if (!context.Features.TryGet(out EnvironmentFeature? environment))
        {
            return project.Validation;
        }

        var validation = environment.ActiveEnvironment.Validation;

        return project.Validation?.Merge(validation) ?? validation;
    }
}
