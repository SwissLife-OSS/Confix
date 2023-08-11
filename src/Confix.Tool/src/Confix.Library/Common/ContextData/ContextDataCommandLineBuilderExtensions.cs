using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Runtime.CompilerServices;
using Confix.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool;

public static class ContextDataCommandLineBuilderExtensions
{
    private static readonly ConditionalWeakTable<CommandLineBuilder, ContextData> _contextData =
        new();

    public static CommandLineBuilder AddContextData(this CommandLineBuilder builder)
    {
        var contextData = new ContextData();
        builder.AddSingleton(contextData);
        _contextData.Add(builder, contextData);

        return builder;
    }

    public static CommandLineBuilder SetContextData<T>(
        this CommandLineBuilder builder,
        Context.Key<T> key,
        T value)
        where T : notnull
    {
        builder.AddMiddleware((context, next) =>
        {
            var data = context.BindingContext.GetRequiredService<ContextData>();
            data.Data.Set(key, value);

            return next(context);
        });

        return builder;
    }

    public static IDictionary<string, object> GetContextData(this InvocationContext context)
    {
        return context.BindingContext.GetRequiredService<ContextData>().Data;
    }

    public static IDictionary<string, object> GetContextData(this CommandLineBuilder builder)
    {
        return _contextData.GetOrCreateValue(builder).Data;
    }

    public static void SetContextData<T>(
        this InvocationContext context,
        in Context.Key<T> key,
        T value)
        where T : notnull
    {
        var data = context.BindingContext.GetRequiredService<ContextData>();
        data.Data.Set(key, value);
    }

    private class ContextData
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }
}
