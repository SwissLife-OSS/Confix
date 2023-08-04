using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares;

public interface IConfigurationFileContext
{
    ConfigurationFileDefinition Definition { get; }

    ProjectDefinition Project { get; }

    IConsoleLogger Logger { get; }
}
