using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Reporting;

namespace Confix.Tool.Middlewares.Reporting;

public sealed class LoadDependencyAnalyzerMiddleware : IMiddleware
{
    private readonly IDependencyProviderFactory _factory;

    public LoadDependencyAnalyzerMiddleware(IDependencyProviderFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        var definitions = configuration.Reporting?.Dependencies?.Providers ??
            Array.Empty<DependencyProviderDefinition>();

        var analyzer = DependencyAnalyzer.FromDefinitions(_factory, definitions);

        context.Features.Set(new DependencyAnalyzerFeature(analyzer));

        context.Logger.LoadedComponentInput();

        return next(context);
    }
}

file static class Log
{
    public static void LoadedComponentInput(this IConsoleLogger console)
    {
        console.Success("Dependency providers loaded");
    }
}
