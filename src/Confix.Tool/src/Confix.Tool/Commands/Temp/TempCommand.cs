using System.CommandLine;

namespace Confix.Tool.Commands.Temp;

public sealed class TempCommand : Command
{
    public TempCommand() : base("temp")
    {
        Description = "This command is temporary and will be removed in the future";
        AddCommand(new CompileSchemaCommand());
        AddCommand(new ComposeCommand());
    }
}
