using System.CommandLine;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Component;

public sealed class BuildComponentCommand : Command
{
    public BuildComponentCommand() : base("build")
    {
        this.SetHandler(
            ExecuteAsync,
            Bind.FromServiceProvider<IServiceProvider>(),
            Bind.FromServiceProvider<CancellationToken>());
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
        => await PipelineBuilder
            .From(services)
            .Use<LoadConfigurationMiddleware>()
            .Use<ExecuteComponentInput>()
            .Use<ExecuteComponentOutput>()
            .BuildExecutor()
            .ExecuteAsync(cancellationToken);
}
