using System.CommandLine;
using System.CommandLine.Parsing;

namespace Confix.Tool.Common.Pipelines;

public static class ParameterCollectionExtensions
{
    public static T Get<T>(this IParameterCollection collection, Option<T> option)
    {
        if (collection.TryGet(option, out object? value) && value is T valueOfT)
        {
            return valueOfT;
        }

        throw new KeyNotFoundException($"Parameter '{option.Name}' was not found.");
    }

    public static bool TryGet<T>(
        this IParameterCollection collection,
        Option<T> option,
        out T value)
    {
        if (collection.TryGet(option, out object? val) && val is T valueOfT)
        {
            value = valueOfT;
            return true;
        }

        value = default!;
        return false;
    }

    public static T Get<T>(this IParameterCollection collection, Argument<T> argument)
    {
        if (collection.TryGet(argument, out object? value) && value is T valueOfT)
        {
            return valueOfT;
        }

        throw new KeyNotFoundException($"Parameter '{argument.Name}' was not found.");
    }

    public static bool TryGet<T>(
        this IParameterCollection collection,
        Argument<T> argument,
        out T value)
    {
        collection.TryGet(argument, out object? val);
        
        if (val is T valueOfT)
        {
            value = valueOfT;
            return true;
        }
        
        if (val is Token { Type: TokenType.Argument } token)
        {
            value = (T)Activator.CreateInstance(typeof(T), token.Value)!;
            return true;
        }

        value = default!;
        return false;
    }
}
