using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public sealed class EnvironmentMiddleware : IMiddleware
{
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        if (context.Features.TryGet(out EnvironmentFeature _))
        {
            return next(context);
        }

        var activeEnvironment = ResolveFromArgument(context) ??
            ResolveFromConfiguration(context) ??
            EnvironmentDefinition.Default;

        context.Logger.EnvironmentResolved(activeEnvironment.Name);
        context.Features.Set(new EnvironmentFeature(activeEnvironment));

        return next(context);
    }

    private static EnvironmentDefinition? ResolveFromArgument(IMiddlewareContext context)
    {
        if (!context.Parameter.TryGet(ActiveEnvironmentOption.Instance, out string environmentName))
        {
            return null;
        }

        var feature = context.Features.Get<ConfigurationFeature>();
        if (feature.Project?.Environments
                .FirstOrDefault(e => e.Name.Equals(environmentName)) is { } env)
        {
            return env;
        }

        context.Logger.EnvironmentNotFound(environmentName);
        throw InvalidEnvironmentConfiguration();
    }

    private static EnvironmentDefinition? ResolveFromConfiguration(IMiddlewareContext context)
    {
        var configurationFeature = context.Features.Get<ConfigurationFeature>();
        var enabledEnvironments =
            configurationFeature.Project?.Environments.Where(e => e.Enabled).ToArray()
            ?? Array.Empty<EnvironmentDefinition>();

        if (enabledEnvironments.Length > 0)
        {
            return enabledEnvironments[0];
        }

        return null;
    }

    private static ExitException InvalidEnvironmentConfiguration()
        => new("Environment configuration invalid");
}

file static class Log
{
    public static void EnvironmentResolved(this IConsoleLogger console, string environment)
        => console.Success("Active Environment is {0}", environment.AsHighlighted());

    public static void EnvironmentNotFound(this IConsoleLogger console, string environment)
        => console.Error("No environment with name {0} configured", environment.AsHighlighted());

    public static void MultipleActiveEnvironments(this IConsoleLogger console)
        => console.Error("Multiple Environments are marked as active");
}
