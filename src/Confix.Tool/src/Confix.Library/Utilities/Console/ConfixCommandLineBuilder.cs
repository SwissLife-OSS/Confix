using System.CommandLine;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool;

/// <summary>
/// Replacement for the <c>CommandLineBuilder</c> type that was removed in System.CommandLine 2.0.
/// Accumulates service registrations into an <see cref="IServiceCollection" /> and exposes the
/// <see cref="RootCommand" /> that is being configured.
/// </summary>
public class ConfixCommandLineBuilder
{
    private static readonly ConditionalWeakTable<Command, IServiceProvider> _providers = new();

    private ServiceProvider? _serviceProvider;

    public ConfixCommandLineBuilder(Command command)
    {
        Command = command;
    }

    /// <summary>
    /// The root command that is being configured.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// The services that are available to commands and middlewares.
    /// </summary>
    public IServiceCollection Services { get; } = new ServiceCollection();

    /// <summary>
    /// Builds the service provider. Repeated calls return the same instance so that singletons
    /// are shared across the whole invocation.
    /// </summary>
    public IServiceProvider BuildServices()
    {
        if (_serviceProvider is null)
        {
            _serviceProvider = Services.BuildServiceProvider();

            // commands are created before the container exists, so the provider is associated
            // with the root command and looked up again when an action is executed.
            _providers.AddOrUpdate(Command, _serviceProvider);
        }

        return _serviceProvider;
    }

    /// <summary>
    /// Resolves the services that belong to the root command of the given parse result.
    /// </summary>
    internal static IServiceProvider GetServices(ParseResult parseResult)
    {
        var rootCommand = parseResult.RootCommandResult.Command;

        if (!_providers.TryGetValue(rootCommand, out var services))
        {
            throw new InvalidOperationException(
                "The command line was not built before it was invoked.");
        }

        return services;
    }
}
