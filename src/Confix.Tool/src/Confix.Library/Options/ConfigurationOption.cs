using System.CommandLine;

namespace Confix.Tool;

internal sealed class ConfigurationOption : Option<string>
{
    public static ConfigurationOption Instance { get; } = new();

    public static string Default => "Debug";

    private ConfigurationOption()
        : base("--configuration")
    {
        Arity = ArgumentArity.ExactlyOne;
        SetDefaultValue(Default);
        Description = "The name of the configuration that would be passed to dotnet " +
                      "build command. Default is Debug.";
    }
}
