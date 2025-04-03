using System.CommandLine;

namespace Confix.Tool;

internal sealed class DotnetConfigurationOptions : Option<string>
{
    public static DotnetConfigurationOptions Instance { get; } = new();

    public static string Default => "Debug";

    private DotnetConfigurationOptions()
        : base("--dotnet-configuration")
    {
        Arity = ArgumentArity.ExactlyOne;
        SetDefaultValue(Default);
        Description = "The configuration passed to dotnet commands. Defaults to 'Debug'.";
    }
}
