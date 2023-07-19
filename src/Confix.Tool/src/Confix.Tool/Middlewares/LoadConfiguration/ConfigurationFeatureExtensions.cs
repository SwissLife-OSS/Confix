using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFeatureExtensions
{
    public static void EnsureProjectScope(this ConfigurationFeature configuration)
    {
        if (configuration.Scope is not ConfigurationScope.Project)
        {
            throw new ExitException("Scope has to be a project");
        }
    }

    public static ProjectDefinition EnsureProject(this ConfigurationFeature configuration)
    {
        if (configuration.Project is not { } project)
        {
            throw new ExitException("No project was found");
        }

        return project;
    }

    public static SolutionDefinition EnsureSolution(this ConfigurationFeature configuration)
    {
        if (configuration.Solution is not { } solution)
        {
            throw new ExitException("No solution was found");
        }

        return solution;
    }
}
