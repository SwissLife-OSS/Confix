using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Entities.Components;

public sealed class MergeComponentProvider : IComponentProvider
{
    public static string Type => "merge";

    /// <inheritdoc />
    public async Task ExecuteAsync(IComponentProviderContext context)
    {
        context.Logger.StartLoadingComponents(context.Project.Name);

        context.EnsureSolution();

        var lookup = context.Project.Components.ToLookup(x => x.Provider + x.ComponentName);

        foreach (var component in context.Components)
        {
            var key = component.Provider + component.ComponentName;
            if (lookup.Contains(key))
            {
                context.Logger.ApplyConfigurationFromFileToComponent(
                    component.Provider,
                    component.ComponentName);

                var componentFromProject = lookup[key].First();

                component.IsEnabled = componentFromProject.IsEnabled;
                component.MountingPoints = componentFromProject.MountingPoints;
                component.Version ??= componentFromProject.Version;
            }
        }
    }
}

file static class Extensions
{
    public static void EnsureSolution(this IComponentProviderContext context)
    {
        if (context.Solution.Directory is not { Exists: true })
        {
            throw new ExitException("A solution directory is required to load components");
        }
    }
}

file static class Log
{
    public static void StartLoadingComponents(this IConsoleLogger logger, string name)
    {
        logger.Debug($"Start loading components from project '{name}'");
    }

    public static void ApplyConfigurationFromFileToComponent(
        this IConsoleLogger logger,
        string provider,
        string componentName)
    {
        logger.Debug(
            $"Apply configuration from file to component '{componentName}' from provider '{provider}'");
    }
}
