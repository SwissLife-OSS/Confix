namespace Confix.Tool;

public static class LinqExtensions
{
    public static void ForEach<T>(
        this IEnumerable<T> source,
        Action<T> action)
    {
        foreach (T element in source)
            action(element);
    }

    public static V? GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key) where K : notnull
    {
        if (dictionary.TryGetValue(key, out V value))
        {
            return value;
        }
        return default;
    }

}
