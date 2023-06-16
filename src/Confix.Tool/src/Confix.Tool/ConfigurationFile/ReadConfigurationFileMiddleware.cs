using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public sealed class ReadConfigurationFileMiddleware : IMiddleware
{
    private readonly IConfigurationFileProviderFactory _factory;

    public ReadConfigurationFileMiddleware(IConfigurationFileProviderFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Loading configuration files...");

        var configuration = context.Features.Get<ConfigurationFeature>();
        var project = configuration.EnsureProject();

        var feature = new ConfigurationFileFeature();

        foreach (var file in project.ConfigurationFiles)
        {
            var provider = _factory.Create(file);

            var factoryContext = new ConfigurationFileContext
            {
                Logger = context.Logger,
                Definition = file,
                Project = project
            };

            foreach (var configurationFile in provider.GetConfigurationFiles(factoryContext))
            {
                feature.Files.Add(configurationFile);
            }
        }

        context.Features.Set(feature);

        await next(context);
    }
}
