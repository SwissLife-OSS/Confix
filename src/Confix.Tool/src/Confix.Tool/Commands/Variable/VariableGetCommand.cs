using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableGetCommand : PipelineCommand<VariableGetPipeline>
{
    public VariableGetCommand() : base("get")
    {
        Description = "resolves a variable by name";
    }
}
