using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableSetCommand : PipelineCommand<VariableSetPipeline>
{
    public VariableSetCommand() : base("set")
    {
        Description = "sets a variable. Overrides existing value if any.";
    }
}