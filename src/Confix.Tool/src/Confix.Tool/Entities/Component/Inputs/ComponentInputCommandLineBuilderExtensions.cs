using System.CommandLine.Builder;
using Confix.Tool.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Entities.Component;

public static class ComponentInputCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterComponentInputs(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new ExecuteComponentInputMiddleware(
                sp.GetRequiredService<IComponentInputFactory>()));

        builder.AddComponentInput<GraphQLComponentInput>();

        return builder;
    }
}
