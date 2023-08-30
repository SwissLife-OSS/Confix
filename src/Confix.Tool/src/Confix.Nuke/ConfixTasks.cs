using Nuke.Common.Tooling;

namespace Confix.Nuke;

public static partial class ConfixTasks
{
    public static void CustomLogger(OutputType outputType, string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine(message);
        }
    }

    internal static string GetToolPath(string? framework = null)
    {
        return ToolPathResolver.GetPackageExecutable(
            packageId: "Confix",
            packageExecutable: "Confix.dll",
            framework: framework);
    }
}

public partial class ConfixComponentBuildSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixComponentInitSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixProjectRestoreSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixProjectBuildSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixProjectInitSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixProjectValidateSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixSolutionRestoreSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixSolutionBuildSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixSolutionInitSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixSolutionValidateSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixVariablesGetSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixVariablesSetSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixVariablesListSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixVariablesCopySettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixBuildSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixComponentListSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixRestoreSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixValidateSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixEncryptSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixDecryptSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixConfigShowSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixConfigSetSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}

public partial class ConfixConfigListSettings
{
    private string GetProcessToolPath()
    {
        return ConfixTasks.GetToolPath(Framework);
    }
}
