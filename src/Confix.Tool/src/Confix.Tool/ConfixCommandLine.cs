using System.CommandLine.Builder;
using ConfiX.Entities.Project.Extensions;
using ConfiX.Entities.Schema.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool;

internal sealed class ConfixCommandLine : CommandLineBuilder
{
    public ConfixCommandLine() : base(new ConfixRootCommand())
    {
        this
            .AddProjectServices()
            .AddSchemaServices()
            .RegisterMiddlewares()
            .RegisterConfigurationAdapters()
            .AddSingleton(DefaultConsole.Create())
            .UseDefaults()
            .UseVerbosity()
            .AddExceptionHandler();
    }
}
