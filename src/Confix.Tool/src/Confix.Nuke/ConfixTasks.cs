using Nuke.Common.Tooling;

namespace Confix.Nuke;

[NuGetTool(Id = PackageId, Executable = Executable)]
public partial class ConfixTasks
{
    public const string PackageId = "Confix";
    public const string Executable = "Confix.dll";

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
            packageId: PackageId,
            packageExecutable: Executable,
            framework: framework);
    }
}

public partial class ConfixComponentBuildSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixComponentAddSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixComponentInitSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixProjectRestoreSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixProjectBuildSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixProjectInitSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixProjectValidateSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixSolutionRestoreSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixSolutionBuildSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixSolutionInitSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixSolutionValidateSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixVariableGetSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixVariableSetSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixVariableListSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixVariableCopySettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixBuildSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixComponentListSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixRestoreSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixValidateSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixEncryptSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixDecryptSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixConfigShowSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixConfigSetSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixConfigListSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}

public partial class ConfixProjectReportSettings
{
    private string GetProcessToolPath()
    {
        return new ConfixTasks().GetToolPath(Framework);
    }
}
