using System.CommandLine;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectCommand : Command
{
    public ProjectCommand() : base("project")
    {
        Description = "This command is used to manage projects.";

        AddCommand(new ProjectRestoreCommand());
        AddCommand(new ProjectBuildCommand());
        AddCommand(new ProjectInitCommand());
        AddCommand(new ProjectValidateCommand());
        AddCommand(new ProjectReportCommand());
    }
}
