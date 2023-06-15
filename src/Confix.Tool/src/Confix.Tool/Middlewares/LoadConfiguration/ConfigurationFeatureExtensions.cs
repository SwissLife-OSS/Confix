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

    public static SolutionDefinition EnsureSolution(this ConfigurationFeature configuration)
    {
        if (configuration.Solution is not { } solution)
        {
            App.Log.NoSolutionWasFound();
            throw new ExitException();
        }

        return solution;
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

    public static void NoSolutionWasFound(this IConsoleLogger console)
    {
        console.Error("No solution was found");
    }
}
