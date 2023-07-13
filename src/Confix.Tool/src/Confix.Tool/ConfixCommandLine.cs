using System.CommandLine.Builder;
using ConfiX.Entities.Project.Extensions;
using ConfiX.Entities.Schema.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ExecutionContext = Confix.Tool.Common.Pipelines.ExecutionContext;

namespace Confix.Tool;

public sealed class ConfixCommandLine : CommandLineBuilder
{
    public ConfixCommandLine() : base(new ConfixRootCommand())
    {
        this
            .AddProjectServices()
            .AddSchemaServices()
            .RegisterMiddlewares()
            .AddSingleton(DefaultConsole.Create())
            .AddSingleton<IServiceProvider>(sp => sp)
            .AddSingleton<IExecutionContext>(_ => ExecutionContext.Create())
            .UseDefaults()
            .UseVerbosity()
            .AddExceptionHandler();
    }
}
