using System.CommandLine;

namespace Confix.Tool.Middlewares;

internal sealed class ActiveEnvironmentOption : Option<string>
{
    public static ActiveEnvironmentOption Instance { get; } = new();

    private ActiveEnvironmentOption()
        : base("--environment")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the environment to run the command in. Overrules the active environment set in .confixrc";

        AddAlias("--env");
    }
}
