using System.CommandLine.Builder;
using Confix.Tool.Common.Pipelines;
using static System.CommandLine.Invocation.MiddlewareOrder;

namespace Confix.Tool;

public static partial class CommandLineBuilderExtensions
{
    public static CommandLineBuilder AddSingleton<T, TImpl>(this CommandLineBuilder builder)
        where TImpl : T, new()
    {
        var value = default(T);

        builder.AddSingleton<T>(_ => value ??= new TImpl());

        return builder;
    }

    public static CommandLineBuilder AddSingleton<T>(this CommandLineBuilder builder)
        where T : new()
    {
        builder.AddSingleton(_ => new T());
        return builder;
    }

    public static CommandLineBuilder AddSingleton<T>(this CommandLineBuilder builder, T instance)
    {
        builder.AddSingleton(_ => instance);
        return builder;
    }

    public static CommandLineBuilder AddSingleton<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        builder.AddMiddleware(x =>
            {
                var cache = default(T);
                x.BindingContext.AddService(sp => cache ??= factory(sp));
            },
            Configuration);
        return builder;
    }

    public static CommandLineBuilder AddTransient<T, TImpl>(this CommandLineBuilder builder)
        where TImpl : T, new()
    {
        T? value = default(T);

        builder.AddTransient<T>(_ => value ??= new TImpl());

        return builder;
    }

    public static CommandLineBuilder AddTransient<T>(this CommandLineBuilder builder)
        where T : class, new()
    {
        builder.AddTransient(_ => new T());
        return builder;
    }

    public static CommandLineBuilder AddTransient<T>(this CommandLineBuilder builder, T instance)
    {
        builder.AddTransient(_ => instance);
        return builder;
    }

    public static CommandLineBuilder AddTransient<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        builder.AddMiddleware(x => x.BindingContext.AddService(factory), Configuration);
        return builder;
    }
}
