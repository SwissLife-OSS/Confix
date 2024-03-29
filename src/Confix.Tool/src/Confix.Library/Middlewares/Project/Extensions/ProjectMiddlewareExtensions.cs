using System.CommandLine.Builder;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Reporting;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Project;

public static class ProjectMiddlewareExtensions
{
    public static CommandLineBuilder RegisterProjectMiddlewares(this CommandLineBuilder builder)
        => builder
            .AddTransient(sp => new ValidationMiddleware(sp.GetRequiredService<ISchemaStore>()))
            .AddTransient(sp => new RestoreProjectMiddleware(
                sp.GetRequiredService<IProjectComposer>(),
                sp.GetRequiredService<ISchemaStore>()))
            .AddTransient(sp =>
                new InitializeConfigurationDefaultValues(sp.GetRequiredService<ISchemaStore>()))
            .AddTransient<BuildComponentsOfProjectMiddleware>()
            .AddTransient<InitProjectMiddleware>()
            .AddTransient<BuildProjectMiddleware>()
            .AddTransient(sp => new ProjectReportMiddleware(
                sp.GetRequiredService<IGitService>(),
                sp.GetRequiredService<ISchemaStore>()));
}
