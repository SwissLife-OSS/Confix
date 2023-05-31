using System.Diagnostics;
using System.Xml.Linq;

namespace Confix.Tool.Commands.Temp;

public static class DotnetHelpers
{
    public static async Task BuildProjectAsync(string path, CancellationToken cancellationToken)
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

        Console.WriteLine(output);

        await process.WaitForExitAsync(cancellationToken); // Wait for the build process to finish

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Build failed.");
        }
        else
        {
            Console.WriteLine("Build succeeded.");
        }
    }

    public static FileInfo? GetAssemblyFileFromCsproj(string csprojPath)
    {
        // Load the .csproj file as an XDocument
        XDocument csprojDoc = XDocument.Load(csprojPath);

        XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

        // Extract the assembly name and target framework from the .csproj file
        var propertyGroup =
            csprojDoc.Element(msbuild + "Project")
                ?.Element(msbuild + "PropertyGroup")
                ?.Element(msbuild + "AssemblyName")
                ?.Value ??
            Path.GetFileNameWithoutExtension(csprojPath);

        // Construct the path to where the assembly should be built
        return GetAssemblyInPathByName(Path.GetDirectoryName(csprojPath)!, propertyGroup);
    }

    public static FileInfo? GetAssemblyInPathByName(string projectDirectory, string assemblyName)
    {
        var binDirectory = Path.Join(projectDirectory, "bin");
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

    public static string? FindProjectFileInPath(string directoryPath)
        => Directory
            .EnumerateFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
}
