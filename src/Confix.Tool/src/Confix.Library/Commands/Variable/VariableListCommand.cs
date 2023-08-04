using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableListCommand : PipelineCommand<VariableListPipeline>
{
    public VariableListCommand() : base("list")
    {
        Description = "list available variables";
    }
}
