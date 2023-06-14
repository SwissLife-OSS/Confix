using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ConfiX.Variables;

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
            .AddArgument(VariableNameArgument.Instance)
            .AddArgument(VariableValueArgument.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        var variableName = context.Parameter.Get(VariableNameArgument.Instance);
        var variableValue = context.Parameter.Get(VariableValueArgument.Instance);

        if (VariablePath.TryParse(variableName, out var parsed) && parsed.HasValue)
        {
            var result = await resolver
                .SetVariable(
                    parsed.Value.ProviderName,
                    parsed.Value.Path,
                    JsonValue.Create(variableValue),
                    context.CancellationToken);

            context.Logger.VariableSet(result);
        }
        else
        {
            context.Logger.InvalidVariableName(variableName);
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
