using System.CommandLine.Builder;
using Confix.Tool.Middlewares.Reporting;
using Confix.Tool.Reporting;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class DependencyAnalyzerCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterDependencyAnalyzerMiddleware(
        this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new LoadDependencyAnalyzerMiddleware(
                sp.GetRequiredService<IDependencyProviderFactory>()));

        return builder;
    }
}
