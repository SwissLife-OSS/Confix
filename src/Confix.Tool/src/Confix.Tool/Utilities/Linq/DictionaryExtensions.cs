using System.Diagnostics.CodeAnalysis;

namespace Confix.Tool;

public static class DictionaryExtensions
{
    public static T GetOrAdd<T>(
        this IDictionary<string, object> dictionary,
        string key,
        Func<T> factory)
    {
        if (dictionary.TryGetValue(key, out var value) && value is T result)
        {
            return result;
        }

        result = factory();
        dictionary.Add(key, result!);
        return result;
    }

    public static bool TryGetValue<T>(
        this IDictionary<string, object> dictionary,
        string key,
        [NotNullWhen(true)] out T value)
    {
        if (dictionary.TryGetValue(key, out var result) && result is T resultT)
        {
            value = resultT;
            return true;
        }

        value = default;
        return false;
    }
}
