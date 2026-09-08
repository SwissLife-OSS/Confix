using System.Runtime.CompilerServices;
using Confix.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool;

public static class ContextDataCommandLineBuilderExtensions
{
    private static readonly ConditionalWeakTable<ConfixCommandLineBuilder, ContextData>
        _contextData = new();

    public static ConfixCommandLineBuilder AddContextData(this ConfixCommandLineBuilder builder)
    {
        // The same instance is shared between configuration time and invocation time so that
        // values set while building the command line are visible to the running pipeline.
        var contextData = _contextData.GetOrCreateValue(builder);
        builder.AddSingleton(contextData);

        return builder;
    }

    public static ConfixCommandLineBuilder SetContextData<T>(
        this ConfixCommandLineBuilder builder,
        Context.Key<T> key,
        T value)
        where T : notnull
    {
        _contextData.GetOrCreateValue(builder).Data.Set(key, value);

        return builder;
    }

    public static IDictionary<string, object> GetContextData(this IServiceProvider services)
        => services.GetRequiredService<ContextData>().Data;

    public static IDictionary<string, object> GetContextData(
        this ConfixCommandLineBuilder builder)
        => _contextData.GetOrCreateValue(builder).Data;

    public static void SetContextData<T>(
        this IServiceProvider services,
        in Context.Key<T> key,
        T value)
        where T : notnull
        => services.GetRequiredService<ContextData>().Data.Set(key, value);

    internal class ContextData
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }
}
