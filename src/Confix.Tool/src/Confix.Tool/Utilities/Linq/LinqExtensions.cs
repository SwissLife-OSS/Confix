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

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(x => x is not null)!;
    }
}
