using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

public sealed class PipelineExecutor
{
    private readonly Pipeline _pipeline;
    private readonly Dictionary<string, object> _parameter = new();

    public PipelineExecutor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public PipelineExecutor AddParameter<TArgument>(string arg, TArgument argument)
    {
        _parameter.Add(arg, argument!);
        return this;
    }

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var context = new MiddlewareContext
        {
            CancellationToken = cancellationToken,
            Parameter = ParameterCollection.From(_parameter),
            Console = _pipeline.Services.GetRequiredService<IAnsiConsole>(),
            Execution = ExecutionContext.Create()
        };
         
        await _pipeline.ExecuteAsync(context);

        return context.ExitCode;
    }
}
