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

        EnvironmentDefinition? activeEnvironment = 
            ResolveFromArgument(context) ?? ResolveFromConfiguration(context);

        if (activeEnvironment is not null)
        {
            context.Logger.EnvironmentResolved(activeEnvironment.Name);
            context.Features.Set(new EnvironmentFeature(activeEnvironment));
        }

        return next(context);
    }

    private static EnvironmentDefinition? ResolveFromArgument(IMiddlewareContext context)
    {
        if (context.Parameter.TryGet(ActiveEnvironmentOption.Instance, out string environmentName))
        {
            if (context.Features.Get<ConfigurationFeature>()
                    .Project?
                    .Environments
                    .FirstOrDefault(e => e.Name.Equals(environmentName)) is { } env)
            {
                return env;
            }

            context.Logger.EnvironmentNotFound(environmentName);
            throw InvalidEnvironmentConfiguration();
        }

        return null;
    }

    private static EnvironmentDefinition? ResolveFromConfiguration(IMiddlewareContext context)
    {
        ConfigurationFeature configurationFeature = context.Features.Get<ConfigurationFeature>();
        var enabledEnvironments =
            configurationFeature.Project?.Environments.Where(e => e.Enabled).ToArray()
            ?? Array.Empty<EnvironmentDefinition>();

        if (enabledEnvironments.Length == 0)
        {
            context.Logger.EnvironmentNotSet();

            return null;
        }

        if (enabledEnvironments.Length == 1)
        {
            return enabledEnvironments[0];
        }

        context.Logger.MultipleActiveEnvironments();
        throw InvalidEnvironmentConfiguration();
    }

    private static ExitException InvalidEnvironmentConfiguration()
        => new ExitException("Environmentconfiguration invalid");
}

file static class Log
{
    public static void EnvironmentResolved(this IConsoleLogger console, string environment)
        => console.Success("Active Environment is {0}", environment.AsHighlighted());

    public static void EnvironmentNotFound(this IConsoleLogger console, string environment)
        => console.Error("No environment with name {0} configured", environment.AsHighlighted());

    public static void MultipleActiveEnvironments(this IConsoleLogger console)
        => console.Error("Multiple Environments are marked as active");

    public static void EnvironmentNotSet(this IConsoleLogger console)
        => console.Error(
            $"No active environment set. Use --environment or set one environment in .confixrc as active");
}
