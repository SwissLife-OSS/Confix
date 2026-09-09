using Confix.Tool;
using Confix.Tool.Entities.Components.DotNet;

namespace Confix.Entities.Project.Extensions;

public static class ProjectCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder AddProjectServices(this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton<IProjectComposer, ProjectComposer>();

        return builder;
    }
}
