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

    public static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    public static async Task<List<T>> ToListAsync<T>(
        this IEnumerable<Task<T>> sources,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        var tasks = sources.ToList();
        while (!cancellationToken.IsCancellationRequested && await Task.WhenAny(tasks) is { } task)
        {
            list.Add(await task);
            tasks.Remove(task);
        }

        return list;
    }
}
