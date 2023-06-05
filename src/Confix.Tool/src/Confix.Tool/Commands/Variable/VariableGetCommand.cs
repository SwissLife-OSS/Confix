using System.CommandLine;
using System.CommandLine.Completions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
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
            VariableNameArgument.Instance);
    }

    public override string? Description => "resolves a variable by name";

    private static async Task<int> ExecuteAsync(
       IAnsiConsole console,
       string variableName
    )
    {
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