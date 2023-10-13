using System.CommandLine;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Variables;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableListPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .AddOption(VariableProviderNameOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        var resolver = context.Features.Get<VariablesFeature>().Resolver;

        IEnumerable<VariablePath> variables;

        context.Status.Message = "Fetching Variables...";

        if (context.Parameter
            .TryGet(VariableProviderNameOption.Instance, out string? variableProviderName))
        {
            variables =
                await resolver.ListVariables(variableProviderName, context.CancellationToken);
        }
        else
        {
            variables = await resolver.ListVariables(context.CancellationToken);
        }

        context.Logger.PrintVariables(variables);
    }
}

file static class Log
{
    public static void PrintVariables(
        this IConsoleLogger console,
        IEnumerable<VariablePath> variables)
    {
        foreach (var variable in variables)
        {
            console.Information(variable.ToString());
        }
    }
}
