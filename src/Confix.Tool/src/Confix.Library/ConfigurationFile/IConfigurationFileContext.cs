using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares;

public interface IConfigurationFileContext
{
    bool ReadOnly => false;

    ConfigurationFileDefinition Definition { get; }

    ProjectDefinition Project { get; }

    IConsoleLogger Logger { get; }
}
