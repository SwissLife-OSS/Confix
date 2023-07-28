using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ConfiX.Variables;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableGetPipeline : Pipeline
{
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .AddOption(VariableNameOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariableResolverFeature>().Resolver;

        if (!context.Parameter.TryGet(VariableNameOption.Instance, out string variableName))
        {
            variableName = await context.AskAsync<string>("Variable name: ");
        }

        var variablePath = VariablePath.Parse(variableName);

        context.Status.Message = $"Resolving variable {variablePath.ToString().AsHighlighted()}...";

        var result = await resolver
            .ResolveOrThrowAsync(variablePath, context.CancellationToken);

        context.Logger.PrintVariableResolved(variablePath, result.ToJsonString());
    }
}

file static class Log
{
    public static void PrintVariableResolved(
        this IConsoleLogger console,
        VariablePath variablePath,
        string value)
    {
        console.Information($"[green]{variablePath}[/] -> [yellow]{value}[/]");
    }
}
