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
            .AddArgument(VariableNameArgument.Instance)
            .AddOption(VariableProviderNameOption.Instance)
            .UseHandler(InvokeAsync);

        Description = "resolves a variable by name";
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        var variableName = context.Parameter.Get(VariableNameArgument.Instance);
        context.Parameter.TryGet(VariableProviderNameOption.Instance, out string variableProviderName);
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
        console.Information($"[green]{variablePath}[/] -> [yellow]{value}[/]");
    }
}
