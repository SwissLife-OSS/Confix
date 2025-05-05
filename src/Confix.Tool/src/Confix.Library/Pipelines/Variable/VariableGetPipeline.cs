using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Variables;

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
            .AddOption(FormatOption.Instance)
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

        context.Logger.PrintVariableResolved(variablePath, result.ToJsonString());

        context.SetOutput(result);
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
