using Confix.Tool.Entities.Components;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class InMemoryComponentProvider : IComponentProvider
{
    public static string Type => "in-memory";
    
    public List<Confix.Tool.Abstractions.Component> Components { get; } = new();

    /// <inheritdoc />
    public Task ExecuteAsync(IComponentProviderContext context)
    {
        foreach (var component in Components)
        {
            context.Components.Add(component);
        }

        return Task.CompletedTask;
    }
}
