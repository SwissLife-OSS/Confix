using Confix.Tool.Reporting;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectReportCommand : PipelineCommand<ProjectReportPipeline>
{
    public ProjectReportCommand() : base("report")
    {
        Description = "Generates a report for the project";
    }
}
