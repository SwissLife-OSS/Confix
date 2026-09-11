using System.CommandLine;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectCommand : Command
{
    public ProjectCommand() : base("project")
    {
        Description = "This command is used to manage projects.";

        Add(new ProjectRestoreCommand());
        Add(new ProjectBuildCommand());
        Add(new ProjectInitCommand());
        Add(new ProjectValidateCommand());
        Add(new ProjectReportCommand());
    }
}
