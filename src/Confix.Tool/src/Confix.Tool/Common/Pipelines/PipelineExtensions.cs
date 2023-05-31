namespace Confix.Tool.Common.Pipelines;

public static class PipelineExtensions
{
    public static PipelineExecutor BuildExecutor(this PipelineBuilder builder)
        => new(builder.Build());
}
