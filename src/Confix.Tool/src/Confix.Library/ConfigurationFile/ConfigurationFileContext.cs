using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationFileContext : IConfigurationFileContext
{
    public required ConfigurationFileDefinition Definition { get; init; }

    public required ProjectDefinition Project { get; init; }

    public required IConsoleLogger Logger { get; init; }
}
