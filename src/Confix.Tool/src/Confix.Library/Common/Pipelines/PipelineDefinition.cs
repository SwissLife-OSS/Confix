using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

public sealed class PipelineDefinition
{
    public List<Func<IServiceProvider, IMiddleware>> Middlewares { get; set; } = new();

    public HashSet<Argument> Arguments { get; set; } = new();

    public HashSet<Option> Options { get; set; } = new();

    public IDictionary<string, object> ContextData { get; set; } = new Dictionary<string, object>();
}
