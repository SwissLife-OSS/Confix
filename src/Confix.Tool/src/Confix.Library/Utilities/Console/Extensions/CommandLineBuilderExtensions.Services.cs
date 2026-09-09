using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Confix.Tool;

public static partial class CommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder AddSingleton<T, TImpl>(
        this ConfixCommandLineBuilder builder)
        where TImpl : T, new()
        => builder.AddSingleton<T>(_ => new TImpl());

    public static ConfixCommandLineBuilder AddSingleton<T>(this ConfixCommandLineBuilder builder)
        where T : new()
        => builder.AddSingleton(_ => new T());

    public static ConfixCommandLineBuilder AddSingleton<T>(
        this ConfixCommandLineBuilder builder,
        T instance)
        => builder.AddSingleton(_ => instance);

    public static ConfixCommandLineBuilder AddSingleton<T>(
        this ConfixCommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        // Registrations replace earlier ones so that tests can override services.
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(T), sp => factory(sp)!));

        return builder;
    }

    public static ConfixCommandLineBuilder AddTransient<T, TImpl>(
        this ConfixCommandLineBuilder builder)
        where TImpl : T, new()
        => builder.AddTransient<T>(_ => new TImpl());

    public static ConfixCommandLineBuilder AddTransient<T>(this ConfixCommandLineBuilder builder)
        where T : class, new()
        => builder.AddTransient(_ => new T());

    public static ConfixCommandLineBuilder AddTransient<T>(
        this ConfixCommandLineBuilder builder,
        T instance)
        => builder.AddTransient(_ => instance);

    public static ConfixCommandLineBuilder AddTransient<T>(
        this ConfixCommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        builder.Services.Replace(ServiceDescriptor.Transient(typeof(T), sp => factory(sp)!));

        return builder;
    }
}
