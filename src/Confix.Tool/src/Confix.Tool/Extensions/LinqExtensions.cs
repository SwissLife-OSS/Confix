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
}
