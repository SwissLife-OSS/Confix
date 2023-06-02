namespace Confix.Tool.Common.Pipelines;

public sealed class ParameterCollection : IParameterCollection
{
    private readonly IReadOnlyDictionary<string, object> _parameters;

    private ParameterCollection(IReadOnlyDictionary<string, object> parameters)
    {
        _parameters = parameters;
    }

    public T Get<T>(string key)
    {
        if (_parameters.TryGetValue(key, out var value) && value is T valueOfT)
        {
            return valueOfT;
        }

        throw new KeyNotFoundException($"Parameter '{key}' was not found.");
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (_parameters.TryGetValue(key, out var result))
        {
            value = (T) result;
            return true;
        }

        value = default!;
        return false;
    }

    public static IParameterCollection From(IReadOnlyDictionary<string, object> parameters)
    {
        return new ParameterCollection(parameters);
    }
}
