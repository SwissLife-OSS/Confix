using System.Diagnostics;
using System.Xml.Linq;

namespace Confix.Tool.Commands.Temp;

public static class DotnetHelpers
{
    public static async Task<ProcessExecutionResult> BuildProjectAsync(
        FileInfo path,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet", // The dotnet CLI
            Arguments = $"build {path}", // The command to build your project
            RedirectStandardOutput = true, // Redirect output so we can read it
            UseShellExecute = false // Don't use the shell to execute the command
        };

        var process = new Process { StartInfo = startInfo };
        process.Start(); // Start the build process

        // Read the output to see if there were any errors
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken); // Wait for the build process to finish

        return new ProcessExecutionResult(process.ExitCode == 0, output);
    }

    public static FileInfo? GetAssemblyFileFromCsproj(FileInfo projectFile)
    {
        // Load the .csproj file as an XDocument
        var csprojDoc = XDocument.Load(projectFile.FullName);

        XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

        // Extract the assembly name and target framework from the .csproj file
        var propertyGroup =
            csprojDoc.Element(msbuild + "Project")
                ?.Element(msbuild + "PropertyGroup")
                ?.Element(msbuild + "AssemblyName")
                ?.Value ??
            Path.GetFileNameWithoutExtension(projectFile.FullName);

        // Construct the path to where the assembly should be built
        return GetAssemblyInPathByName(projectFile.Directory!, propertyGroup);
    }

    public static FileInfo? GetAssemblyInPathByName(
        DirectoryInfo projectDirectory,
        string assemblyName)
    {
        var binDirectory = Path.Join(projectDirectory.FullName, "bin");
        if (!Directory.Exists(binDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The directory '{binDirectory}' was not found. Make sure to build the project first.");
        }

        var firstMatch = Directory
            .EnumerateFiles(binDirectory, assemblyName + ".dll", SearchOption.AllDirectories)
            .FirstOrDefault();

        return firstMatch is null ? null : new FileInfo(firstMatch);
    }

    public static FileInfo? FindProjectFileInPath(DirectoryInfo directory)
        => Directory
            .EnumerateFiles(directory.FullName, "*.csproj", SearchOption.TopDirectoryOnly)
            .Select(x => new FileInfo(x))
            .FirstOrDefault();
}
