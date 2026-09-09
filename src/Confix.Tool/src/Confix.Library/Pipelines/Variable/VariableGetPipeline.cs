using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Utilities.Json;
using Confix.Variables;
using Spectre.Console;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableGetPipeline : Pipeline
{
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .Add(VariableNameOption.Instance)
            .Add(FormatOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var variableContext = new VariableProviderContext(
            context.Parameter,
            context.CancellationToken);
        
        var resolver = context.Features.Get<VariablesFeature>().Resolver;

        if (!context.Parameter.TryGet(VariableNameOption.Instance, out string variableName))
        {
            variableName = await context.AskAsync<string>("Variable name: ");
        }

        var variablePath = VariablePath.Parse(variableName);

        context.Status.Message = $"Resolving variable {variablePath.ToString().AsHighlighted()}...";

        var result = await resolver
            .ResolveOrThrowAsync(variablePath, variableContext);

        await context.Status.StopAsync();

        context.Logger.PrintVariableResolved(variablePath, ToDisplayValue(result));

        context.SetOutput(result);
    }

    private static string ToDisplayValue(JsonNode result)
        => result is JsonValue value && value.TryGetValue(out string? stringValue)
            ? stringValue
            : result.ToRelaxedJsonString();
}

file static class Log
{
    public static void PrintVariableResolved(
        this IConsoleLogger console,
        VariablePath variablePath,
        string value)
    {
        console.Information(
            $"[green]{variablePath.ToString().EscapeMarkup()}[/] -> [yellow]{value.EscapeMarkup()}[/]");
    }
}
