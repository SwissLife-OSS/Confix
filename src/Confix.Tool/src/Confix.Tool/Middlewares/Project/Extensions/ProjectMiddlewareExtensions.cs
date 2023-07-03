using System.CommandLine.Builder;
using Confix.Tool.Entities.Components.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Project.Extensions;

public static class ProjectMiddlewareExtensions
{
    public static CommandLineBuilder RegisterProjectMiddlewares(this CommandLineBuilder builder)
        => builder
            .AddTransient(sp => new ValidationMiddleware(sp.GetRequiredService<ISchemaStore>()))
            .AddTransient<BuildComponentsOfProjectMiddleware>()
            .AddTransient<InitProjectMiddleware>()
            .AddTransient<BuildProjectMiddleware>();
}
