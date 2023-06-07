using System.CommandLine;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ConfiX.Variables;
using Spectre.Console;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableGetCommand : Command
{
    public VariableGetCommand() : base("get")
    {
        this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .AddArgument(VariableProviderNameArgument.Instance)
            .AddArgument(VariableNameArgument.Instance)
            .UseHandler(InvokeAsync);

        Description = "resolves a variable by name";
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        var variableName = context.Parameter.Get(VariableNameArgument.Instance);
        var variableProviderName = context.Parameter.Get(VariableProviderNameArgument.Instance);
        var variablePath = variableProviderName is null
            ? VariablePath.Parse(variableName)
            : new VariablePath(variableProviderName, variableName);

        var result = await resolver
            .ResolveVariable(variablePath, context.CancellationToken);

        context.Logger.PrintVariableResolved(variablePath, result);
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
