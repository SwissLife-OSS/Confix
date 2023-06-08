using System.CommandLine.Builder;
using Confix.Tool;
using Confix.Tool.Entities.Components.DotNet;

namespace ConfiX.Entities.Project.Extensions;

public static class ProjectCommandLineBuilderExtensions
{
    public static CommandLineBuilder AddProjectServices(this CommandLineBuilder builder)
    {
        builder.AddSingleton<IProjectComposer, ProjectComposer>();

        return builder;
    }
}
