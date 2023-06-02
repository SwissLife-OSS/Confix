using System.CommandLine;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableListCommand : Command
{
    public VariableListCommand() : base("list")
        => this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>();
}
