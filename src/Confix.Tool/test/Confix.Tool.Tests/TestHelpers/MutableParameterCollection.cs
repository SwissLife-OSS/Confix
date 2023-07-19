using System.CommandLine;
using Confix.Tool.Common.Pipelines;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class MutableParameterCollection : IParameterCollection
{
    private readonly Dictionary<Symbol, object?> Parameters = new();

    public T Get<T>(string key)
    {
        foreach (var parameter in Parameters)
        {
            if (parameter.Key.Name == key && parameter.Value is T valueOfT)
            {
                return valueOfT;
            }
        }

        throw new KeyNotFoundException($"Parameter '{key}' was not found.");
    }

    public bool TryGet<T>(string key, out T value)
    {
        foreach (var parameter in Parameters)
        {
            if (parameter.Key.Name == key && parameter.Value is T valueOfT)
            {
                value = valueOfT;
                return true;
            }
        }

        value = default!;
        return false;
    }

    public bool TryGet(Symbol symbol, out object? value)
        => Parameters.TryGetValue(symbol, out value);
}
