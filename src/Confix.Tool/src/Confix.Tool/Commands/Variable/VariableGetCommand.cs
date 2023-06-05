using System.CommandLine;
using System.CommandLine.Completions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableGetCommand : Command
{
    public VariableGetCommand() : base("get")
        => this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>()
            .Use<VariableMiddleware>()
            .AddArgument<string>("variable-name", "The name of the variable to resolve");

    public override string? Description => "resolves a variable by name";
}
