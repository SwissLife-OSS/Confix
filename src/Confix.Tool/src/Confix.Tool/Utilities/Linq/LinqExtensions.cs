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

    public static IReadOnlyList<T>? MergeWith<T>(
        this (IEnumerable<T>?, IEnumerable<T>?) tuple,
        Func<T, T, bool> keySelector,
        Func<T, T, T> combine)
    {
        return tuple switch
        {
            (null, null) => null,
            (null, { } c) => c.ToArray(),
            ({ } c, null) => c.ToArray(),
            ({ } c1, { } c2) => c2.Select(x
                    => c1.FirstOrDefault(y => keySelector(x, y)) is { } current
                        ? combine(current, x)
                        : x)
                .ToArray()
        };
    }

    public static T? SingleOrNone<T>(this IEnumerable<T> enumerable) where T : class?
    {
        T? result = null;
        foreach (var item in enumerable)
        {
            if (result is not null)
            {
                return null;
            }

            result = item;
        }

        return result;
    }
}
