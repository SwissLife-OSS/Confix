using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFeatureExtensions
{
    public static void EnsureProjectScope(this ConfigurationFeature configuration)
    {
        if (configuration.Scope is not ConfigurationScope.Project)
        {
            App.Log.ScopeHasToBeAProject();
            throw new ExitException();
        }
    }

    public static ProjectDefinition EnsureProject(this ConfigurationFeature configuration)
    {
        if (configuration.Project is not { } project)
        {
            App.Log.NoProjectWasFound();
            throw new ExitException();
        }

        return project;
    }

    public static RepositoryDefinition EnsureRepository(this ConfigurationFeature configuration)
    {
        if (configuration.Repository is not { } repository)
        {
            App.Log.NoRepositoryWasFound();
            throw new ExitException();
        }

        return repository;
    }
}

file static class Log
{
    public static void ScopeHasToBeAProject(this IConsoleLogger console)
    {
        console.Error("Scope has to be a project");
    }

    public static void NoProjectWasFound(this IConsoleLogger console)
    {
        console.Error("No project was found");
    }

    public static void NoRepositoryWasFound(this IConsoleLogger console)
    {
        console.Error("No repository was found");
    }
}
