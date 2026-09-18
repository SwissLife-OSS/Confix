using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Spectre.Console;

namespace Confix.Tool.Validation;

/// <summary>
/// Executes the application's option contracts using its runtime and dependencies.
/// </summary>
internal sealed class DotnetOptionsValidator : IConfigurationValidator
{
    public async Task<JsonNode?> ValidateAsync(
        IMiddlewareContext context,
        ValidationConfiguration settings,
        bool exportSchema)
    {
        // --no-restore only skips schema restore, which this validator never reads.
        var directory = context.Features
            .Get<ConfigurationFeature>()
            .EnsureProject()
            .Directory!
            .FullName;

        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        var target = await DotnetOptionsRunner.EnsureTargetAsync(
            context, directory, settings.Framework);

        JsonNode? schema = null;

        foreach (var file in files)
        {
            // Contracts are identical for every input, so the schema is requested only once.
            var request = new JsonObject
            {
                ["version"] = DotnetOptionsRunner.ProtocolVersion,
                ["configuration"] = file.Content?.DeepClone(),
                ["coverage"] = settings.Coverage ?? ValidationConfiguration.Strict,
                ["exportSchema"] = exportSchema && schema is null
            };

            var response = await DotnetOptionsRunner.InvokeAsync(
                context, target, directory, request);

            if (response.ExitCode != 0 || response.Errors.Count > 0)
            {
                var messages = response.Errors.Select(Markup.Escape);

                throw new ExitException(
                    $"Code-first validation failed for {file.InputFile.Name}:\n" +
                    string.Join('\n', messages));
            }

            schema = response.Schema ?? schema;
        }

        return schema;
    }
}
