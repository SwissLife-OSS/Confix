using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;

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
        var csprojDoc = XDocument.Load(projectFile.FullName, LoadOptions.PreserveWhitespace);

        // Extract the assembly name and target framework from the .csproj file
        var propertyGroup =
            csprojDoc.Element(Xml.Project)
                ?.Element(Xml.PropertyGroup)
                ?.Element(Xml.AssemblyName)
                ?.Value ??
            Path.GetFileNameWithoutExtension(projectFile.FullName);

        // Construct the path to where the assembly should be built
        return GetAssemblyInPathByName(projectFile.Directory!, propertyGroup);
    }

    public static string EnsureUserSecretsId(FileInfo csprojFile)
    {
        App.Log.EnsuringUserSecretsIdInTheCsprojFile(csprojFile.FullName);

        var csprojDoc = XDocument.Load(csprojFile.FullName, LoadOptions.PreserveWhitespace);

        var project = csprojDoc.Element(Xml.Project);
        if (project == null)
        {
            App.Log.CsprojFileDoesNotContainAProjectElement(csprojFile.FullName);
            project = new XElement(Xml.Project);
            csprojDoc.Add(project);
        }

        var propertyGroup = project.Elements(Xml.PropertyGroup).FirstOrDefault();
        if (propertyGroup == null)
        {
            App.Log.CsprojFileDoesNotContainAPropertyGroupElement(csprojFile.FullName);
            propertyGroup = new XElement(Xml.PropertyGroup);
            project.Add(propertyGroup);
        }

        var userSecretsId = propertyGroup.Element(Xml.UserSecretsId)?.Value;
        if (userSecretsId == null)
        {
            userSecretsId = Guid.NewGuid().ToString();
            propertyGroup.Add(new XElement(Xml.UserSecretsId, userSecretsId));

            App.Log.AddedUserSecretsIdToTheCsprojFile(userSecretsId);

            csprojDoc.Save(csprojFile.FullName);
        }
        else
        {
            App.Log.UserSecretsIdAlreadyExistsInTheCsprojFile(userSecretsId);
        }

        return userSecretsId;
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

file static class Xml
{
    public const string Project = "Project";
    public const string PropertyGroup = "PropertyGroup";
    public const string AssemblyName = "AssemblyName";
    public const string UserSecretsId = "UserSecretsId";
}

file static class Log
{
    public static void AddedUserSecretsIdToTheCsprojFile(
        this IConsoleLogger console,
        string userSecretsId)
    {
        console.Debug($"Added UserSecretsId '{userSecretsId}' to the csproj file");
    }

    public static void UserSecretsIdAlreadyExistsInTheCsprojFile(
        this IConsoleLogger console,
        string userSecretsId)
    {
        console.Debug($"UserSecretsId '{userSecretsId}' already exists in the csproj file");
    }

    public static void EnsuringUserSecretsIdInTheCsprojFile(
        this IConsoleLogger console,
        string csprojFile)
    {
        console.Debug($"Ensuring UserSecretsId is in the csproj file '{csprojFile}'");
    }

    public static void CsprojFileDoesNotContainAProjectElement(
        this IConsoleLogger console,
        string csprojFile)
    {
        console.Warning(
            $"The csproj file '{csprojFile}' does not contain a Project element. Adding one.");
    }

    public static void CsprojFileDoesNotContainAPropertyGroupElement(
        this IConsoleLogger console,
        string csprojFile)
    {
        console.Warning(
            $"The csproj file '{csprojFile}' does not contain a PropertyGroup element. Adding one.");
    }
}
