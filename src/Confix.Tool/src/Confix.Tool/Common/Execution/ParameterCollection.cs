using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

public sealed class ParameterCollection : IParameterCollection
{
    private readonly IReadOnlyDictionary<Symbol, object?> _parameters;

    private ParameterCollection(IReadOnlyDictionary<Symbol, object?> parameters)
    {
        _parameters = parameters;
    }

    public T Get<T>(string key)
    {
        foreach (var parameter in _parameters)
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
        foreach (var parameter in _parameters)
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
        => _parameters.TryGetValue(symbol, out value);

    public static IParameterCollection From(IReadOnlyDictionary<Symbol, object?> parameters)
    {
        return new ParameterCollection(parameters);
    }

    public static IParameterCollection Empty()
        => From(new Dictionary<Symbol, object?>());
}
