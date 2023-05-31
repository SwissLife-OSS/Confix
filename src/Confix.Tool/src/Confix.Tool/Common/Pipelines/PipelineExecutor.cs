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
            Parameter = ParameterCollection.From(_parameter)
        };

        await _pipeline.ExecuteAsync(context);

        return context.ExitCode;
    }
}
