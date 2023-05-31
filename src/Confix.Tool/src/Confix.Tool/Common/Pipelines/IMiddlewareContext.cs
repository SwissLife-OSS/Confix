namespace Confix.Tool.Common.Pipelines;

public interface IMiddlewareContext
{
    IFeatureCollection Features { get; }

    IParameterCollection Parameter { get; }

    IDictionary<string, object> ContextData { get; }

    CancellationToken CancellationToken { get; }
}