using System.CommandLine;
using System.CommandLine.Help;
using Confix.Tool.Commands;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Config;
using Confix.Tool.Commands.Encryption;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Commands.Variable;

namespace Confix.Tool;

internal sealed class ConfixRootCommand : Command
{
    public ConfixRootCommand() : base("confix")
    {
        // help is no longer added implicitly to a plain command
        Add(new HelpOption());
        Add(new System.CommandLine.VersionOption());

        VerbosityOption.Instance.Recursive = true;
        Add(VerbosityOption.Instance);

        Add(new ComponentCommand());
        Add(new ProjectCommand());
        Add(new SolutionCommand());
        Add(new VariableCommand());

        Add(new BuildCommand());
        Add(new RestoreCommand());
        Add(new ValidateCommand());

        Add(new FileEncryptCommand());
        Add(new FileDecryptCommand());

        Add(new ConfigCommand());
    }
}
