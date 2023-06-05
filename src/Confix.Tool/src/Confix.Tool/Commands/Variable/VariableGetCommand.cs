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
            .AddArgument(VariableNameArgument.Instance)
            .UseHandler(InvokeAsync);
    }

    public override string? Description => "resolves a variable by name";

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        IVariableResolver resolver = context.Features.Get<VariableResolverFeature>().Resolver;
        string variableName = context.Parameter.Get(VariableNameArgument.Instance);
        var result = await resolver.ResolveVariable(VariablePath.Parse(variableName), context.CancellationToken);

        context.Console.WriteLine($"{variableName} -> {result}");
    }
}
