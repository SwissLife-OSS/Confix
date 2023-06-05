using System.CommandLine;
using System.CommandLine.Completions;
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
            .AddArgument(VariableNameArgument.Instance);

        this.SetHandler(
            ExecuteAsync,
            Bind.FromServiceProvider<IAnsiConsole>(),
            Bind.FromServiceProvider<IVariableResolver>(),
            VariableNameArgument.Instance,
            Bind.FromServiceProvider<CancellationToken>());
    }

    public override string? Description => "resolves a variable by name";

    private static async Task<int> ExecuteAsync(
       IAnsiConsole console,
       IVariableResolver resolver,
       string variableName,
       CancellationToken cancellationToken
    )
    {
        var result = await resolver.ResolveVariable(new VariablePath("local", variableName), cancellationToken);
        return ExitCodes.Success;
    }
}


file class VariableNameArgument : Argument<string>
{
    public static VariableNameArgument Instance { get; } = new();

    private VariableNameArgument()
        : base("variable-name")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the variable to resolve";
    }
}