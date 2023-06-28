using System.CommandLine.Builder;
using Confix.Tool.Entities.Components.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Project;

public static class ProjectCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterProjectMiddlewares(this CommandLineBuilder builder)
    {
        builder.AddTransient(sp => new ValidationMiddleware(sp.GetRequiredService<ISchemaStore>()));
        builder.AddTransient(sp => new BuildComponentsOfProjectMiddleware(sp));
        builder.AddTransient<BuildProjectMiddleware>();
        builder.AddTransient(sp => new ReloadProjectMiddleware(
            sp.GetRequiredService<IProjectComposer>(),
            sp.GetRequiredService<ISchemaStore>()));

        return builder;
    }
}
