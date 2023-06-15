using System.CommandLine;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Middlewares;
using Spectre.Console;

namespace Confix.Tool.Commands.Component;

public sealed class BuildComponentCommand : PipelineCommand<BuildComponentPipeline>
{
    public BuildComponentCommand() : base("build")
    {
        Description = "builds a component. Runs all configured component inputs";
    }
}
