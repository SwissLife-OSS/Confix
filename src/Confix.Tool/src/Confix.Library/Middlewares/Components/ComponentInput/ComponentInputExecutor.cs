using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public sealed class ComponentInputExecutor
    : IComponentInputExecutor
{
    private readonly IReadOnlyList<IComponentInput> _inputs;

    public ComponentInputExecutor(IReadOnlyList<IComponentInput> inputs)
    {
        _inputs = inputs;
    }

    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        foreach (var input in _inputs)
        {
            await input.ExecuteAsync(context);
        }
    }

    public static IComponentInputExecutor FromDefinitions(
        IComponentInputFactory componentInputs,
        IEnumerable<ComponentInputDefinition> configurations)
    {
        var inputs = new List<IComponentInput>();

        foreach (var configuration in configurations)
        {
            App.Log.LoadedComponentInput(configuration.Type);
            inputs.Add(componentInputs.CreateInput(configuration));
        }

        if (inputs.Count == 0)
        {
            App.Log.LoadedComponentInput();
        }

        return new ComponentInputExecutor(inputs);
    }
}

file static class Log
{
    public static void LoadedComponentInput(this IConsoleLogger console, string name)
    {
        console.Debug($"Component input '{name}' loaded");
    }

    public static void LoadedComponentInput(this IConsoleLogger console)
    {
        console.Warning(
            "No component inputs loaded because no component inputs were defined. You can define component inputs in the 'confix.json' or the 'confix.solution' file.");
    }
}
