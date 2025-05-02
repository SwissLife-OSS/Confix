using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Variables;
using Spectre.Console;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableSetPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .AddOption(VariableNameOption.Instance)
            .AddOption(VariableValueOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.Features.Get<ConfigurationFeature>().EnsureProjectScope();

        var variableContext = new VariableProviderContext(
            context.Parameter,
            context.CancellationToken);

        var resolver = context.Features.Get<VariablesFeature>().Resolver;
        if (!context.Parameter.TryGet(VariableNameOption.Instance, out string variableName))
        {
            variableName = await context.AskAsync<string>("Variable name: ");
        }

        if (VariablePath.TryParse(variableName, out var parsed))
        {
            if (!context.Parameter.TryGet(VariableValueOption.Instance, out string variableValue))
            {
                variableValue = await context.AskPasswordAsync("Variable value: ");
            }

            var value = JsonValue.Create(variableValue)!;

            context.Status.Message =
                $"Setting variable {parsed.Value.ToString().AsHighlighted()}...";

            var result = await resolver
                .SetVariable(parsed.Value, value, variableContext);

            context.Logger.VariableSet(result);
        }
        else
        {
            throw ThrowHelper.InvalidVariableName(variableName);
        }
    }
}

file static class Log
{
    public static void InvalidVariableName(
        this IConsoleLogger console,
        string variableName)
    {
        console.Error($"Invalid variable name: [red]{variableName}[/]");
        console.Information("Variable name must be like: [blue]$provider:some.path[/]");
    }

    public static void VariableSet(
        this IConsoleLogger console,
        VariablePath variablePath)
    {
        console.Success($"Variable [green]{variablePath}[/] set successfully.");
    }
}
