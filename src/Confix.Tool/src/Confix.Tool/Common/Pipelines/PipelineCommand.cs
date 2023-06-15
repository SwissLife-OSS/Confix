using System.CommandLine;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Commands.Project;

public abstract class PipelineCommand<T> : Command where T : Pipeline, new()
{
    /// <inheritdoc />
    protected PipelineCommand(string name, string? description = null) : base(name, description)
    {
        this.SetPipeline<T>();
    }
}
