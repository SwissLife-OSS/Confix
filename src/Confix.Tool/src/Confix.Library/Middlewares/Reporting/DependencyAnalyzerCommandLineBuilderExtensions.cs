using Confix.Tool.Middlewares.Reporting;
using Confix.Tool.Reporting;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class DependencyAnalyzerCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder RegisterDependencyAnalyzerMiddleware(
        this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new LoadDependencyAnalyzerMiddleware(
                sp.GetRequiredService<IDependencyProviderFactory>()));

        return builder;
    }
}
