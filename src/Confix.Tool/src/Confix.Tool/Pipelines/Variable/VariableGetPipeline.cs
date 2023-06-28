using System.CommandLine;
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
            .AddArgument(VariableNameArgument.Instance)
            .AddOption(VariableProviderNameOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        var variableName = context.Parameter.Get(VariableNameArgument.Instance);
        context.Parameter.TryGet(VariableProviderNameOption.Instance,
            out string variableProviderName);
        var variablePath = variableProviderName is null
            ? VariablePath.Parse(variableName)
            : new VariablePath(variableProviderName, variableName);

        var result = await resolver
            .ResolveVariable(variablePath, context.CancellationToken);

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
        console.Information(
            $"{variablePath.ToString().AsVariableName()} -> {value.AsVariableValue()}");
    }
}
