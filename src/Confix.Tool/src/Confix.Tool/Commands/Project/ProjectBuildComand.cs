using System.CommandLine;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectBuildCommand : Command
{
    public ProjectBuildCommand() : base("build")
    {
        this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>()
            .UseConfigurationFiles()
            .UseEnvironment()
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>();

        Description = "Replaces all variables in the project files with their values";
    }
}
