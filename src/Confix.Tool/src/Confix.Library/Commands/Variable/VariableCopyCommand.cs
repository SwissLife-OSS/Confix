using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableCopyCommand : PipelineCommand<VariableCopyPipeline>
{
    public VariableCopyCommand() : base("copy")
    {
        Description = "Copies a variable from one provider to another provider";
    }
}
