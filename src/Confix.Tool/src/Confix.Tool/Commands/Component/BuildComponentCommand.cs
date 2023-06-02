using System.CommandLine;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Component;

public sealed class BuildComponentCommand : Command
{
    public BuildComponentCommand() : base("build")
        => this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>()
            .Use<ExecuteComponentInput>()
            .Use<ExecuteComponentOutput>();
}
