using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Confix.Tool;

namespace ConfiX.Inputs;

public static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder AddTestService<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
    {
        builder.AddMiddleware(x =>
            {
                T cache = default(T);
                x.BindingContext.AddService(sp => cache ??= factory(sp));
            },
            MiddlewareOrder.Configuration + 10);
        return builder;
    }
}
