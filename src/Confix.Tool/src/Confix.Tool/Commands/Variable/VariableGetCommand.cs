using System.CommandLine;
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
            .Use<VariableMiddleware>()
            .AddArgument(VariableProviderNameArgument.Instance)
            .AddArgument(VariableNameArgument.Instance)
            .UseHandler(InvokeAsync);

        Description = "resolves a variable by name";
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        IVariableResolver resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        string variableName = context.Parameter.Get(VariableNameArgument.Instance);
        string? variableProviderName = context.Parameter.Get(VariableProviderNameArgument.Instance);
        VariablePath variablePath =
            variableProviderName is null ?
                VariablePath.Parse(variableName)
                : new VariablePath(variableProviderName, variableName);

        var result = await resolver.ResolveVariable(
            variablePath,
            context.CancellationToken);

        context.Console.WriteLine($"{variablePath} -> {result}");
    }
}
