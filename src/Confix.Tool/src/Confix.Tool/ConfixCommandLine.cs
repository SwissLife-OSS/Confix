using System.CommandLine.Builder;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool;

internal sealed class ConfixCommandLine : CommandLineBuilder
{
    public ConfixCommandLine() : base(new ConfixRootCommand())
    {
        this
            .RegisterMiddlewares()
            .AddSingleton(DefaultConsole.Create())
            .AddSingleton<IProjectDiscovery, ProjectDiscovery>()
            .UseDefaults()
            .UseVerbosity()
            .AddExceptionHandler();
    }
}
