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
            .AddSingleton(DefaultConsole.Create())
            .AddSingleton<IServiceProvider>(sp => sp)
            .UseDefaults()
            .UseVerbosity()
            .AddExceptionHandler();
    }
}
