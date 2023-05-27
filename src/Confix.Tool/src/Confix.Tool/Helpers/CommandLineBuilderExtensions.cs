using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace Confix.Tool;

internal static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder AddService<T, TImpl>(this CommandLineBuilder builder)
        where TImpl : T, new()
    {
        T? value = default(T);

        builder.AddService<T>(_ => value ??= new TImpl());

        return builder;
    }

    public static CommandLineBuilder AddService<T>(this CommandLineBuilder builder, T instance)
    {
        builder.AddService(_ => instance);
        return builder;
    }

    public static CommandLineBuilder AddService<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        builder.AddMiddleware(x =>
            {
                T cache = default(T);
                x.BindingContext.AddService(sp => cache ??= factory(sp));
            },
            MiddlewareOrder.Configuration);
        return builder;
    }
}
