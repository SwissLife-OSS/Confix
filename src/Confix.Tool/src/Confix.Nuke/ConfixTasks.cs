using Nuke.Common.Tooling;

namespace Confix.Nuke;

public partial class ConfixTasks
{
    public static void CustomLogger(OutputType outputType, string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine(message);
        }
    }

    internal string GetToolPath(string? framework = null)
    {
        return NuGetToolPathResolver.GetPackageExecutable(
            packageId: "Confix",
            packageExecutable: "Confix.dll",
            framework: framework);
    }
}
