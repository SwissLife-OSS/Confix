using Confix.Tool;

namespace Confix.Inputs;

public static class CommandLineBuilderExtensions
{
    /// <summary>
    /// Registers a test service. The factory is invoked on every resolve so that tests can swap
    /// the underlying instance between runs (see <c>TestConfixCommandline.ResetConsole</c>).
    /// </summary>
    public static ConfixCommandLineBuilder AddTestService<T>(
        this ConfixCommandLineBuilder builder,
        Func<IServiceProvider, T> factory)
        => builder.AddTransient(factory);
}
