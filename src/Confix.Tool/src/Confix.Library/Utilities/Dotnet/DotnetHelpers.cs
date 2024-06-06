using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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

    public static string GetAssemblyNameFromCsproj(FileInfo projectFile)
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

        return propertyGroup;
    }

    public static async Task<string> EnsureUserSecretsIdAsync(
        FileInfo csprojFile,
        CancellationToken ct)
    {
        App.Log.EnsuringUserSecretsIdInTheCsprojFile(csprojFile.FullName);

        var csprojDoc = XDocument.Load(csprojFile.FullName, LoadOptions.None);

        var project = csprojDoc.Element(Xml.Project);
        if (project == null)
        {
            App.Log.CsprojFileDoesNotContainAProjectElement(csprojFile.FullName);
            project = new XElement(Xml.Project);
            csprojDoc.Add(project);
        }

        var propertyGroups = project.Elements(Xml.PropertyGroup).ToArray();
        if (propertyGroups.Length == 0)
        {
            App.Log.CsprojFileDoesNotContainAPropertyGroupElement(csprojFile.FullName);
            var group = new XElement(Xml.PropertyGroup);
            project.Add(group);
            propertyGroups = new[] { group };
        }

        var userSecretsId = propertyGroups.Select(x => x.Element(Xml.UserSecretsId))
            .OfType<XElement>()
            .FirstOrDefault()
            ?.Value;

        if (userSecretsId == null)
        {
            var propertyGroup = propertyGroups.First();

            userSecretsId = Guid.NewGuid().ToString();

            propertyGroup.Add(new XElement(Xml.UserSecretsId, userSecretsId));

            App.Log.AddedUserSecretsIdToTheCsprojFile(userSecretsId);

            await csprojDoc.PrettifyAndSaveAsync(csprojFile.FullName, ct);
        }
        else
        {
            App.Log.UserSecretsIdAlreadyExistsInTheCsprojFile(userSecretsId);
        }

        return userSecretsId;
    }

    public static async Task EnsureEmbeddedResourceAsync(
        FileInfo csprojFile,
        string path,
        CancellationToken ct)
    {
        App.Log.EnsureEmbeddedResourceExists(csprojFile.FullName);

        var csprojDoc = XDocument.Load(csprojFile.FullName, LoadOptions.None);

        var project = csprojDoc.Element(Xml.Project);
        if (project == null)
        {
            App.Log.CsprojFileDoesNotContainAProjectElement(csprojFile.FullName);
            throw new XmlException(
                $"The .csproj file '{csprojFile.FullName}' does not contain a <Project> element.");
        }

        var propertyGroups = project.Elements(Xml.ItemGroup).ToArray();

        var embeddedResources = propertyGroups
            .Select(x => x.Element(Xml.EmbeddedResource))
            .OfType<XElement>()
            .FirstOrDefault(x => x.Attribute("Include")?.Value == path)
            ?.Value;

        if (embeddedResources == null)
        {
            var itemGroup = new XElement(Xml.ItemGroup);
            project.Add(itemGroup);

            var embeddedResource = new XElement(Xml.EmbeddedResource);
            itemGroup.Add(embeddedResource);

            embeddedResource.Add(new XAttribute("Include", path));

            App.Log.AddedEmbeddedResourceToTheCsprojFile(path);

            await csprojDoc.PrettifyAndSaveAsync(csprojFile.FullName, ct);
        }
        else
        {
            App.Log.EmbeddedResourceAlreadyExistsInTheCsprojFile(path);
        }
    }

    public static PathAssemblyResolver CreateAssemblyResolver(DirectoryInfo projectDirectory)
    {
        var binDirectory = Path.Join(projectDirectory.FullName, "bin");
        if (!Directory.Exists(binDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The directory '{binDirectory}' was not found. Make sure to build the project first.");
        }

        var appAssembly = Directory
            .EnumerateFiles(binDirectory, "*.dll", SearchOption.AllDirectories)
            .DistinctBy(Path.GetFileName);

        var runtimeAssemblies = Directory
            .GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        return new PathAssemblyResolver(
            appAssembly.Concat(runtimeAssemblies)
            .GroupBy(Path.GetFileName)
            .Select(p=>p.First())
            .ToArray());
    }

    public static FileInfo? FindProjectFileInPath(DirectoryInfo directory)
        => Directory
            .EnumerateFiles(directory.FullName, "*.csproj", SearchOption.TopDirectoryOnly)
            .Select(x => new FileInfo(x))
            .FirstOrDefault();

    private static async Task PrettifyAndSaveAsync(
        this XDocument xDocument,
        string filePath,
        CancellationToken ct)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.Async = true;
        settings.OmitXmlDeclaration = true;

        var formattedCsproj = new StringBuilder();
        await using var writer = XmlWriter.Create(formattedCsproj, settings);
        await xDocument.WriteToAsync(writer, ct);
        await writer.FlushAsync();

        await File.WriteAllTextAsync(filePath, formattedCsproj.ToString(), ct);
    }
}

file static class Xml
{
    public const string Project = "Project";
    public const string ItemGroup = "ItemGroup";
    public const string PropertyGroup = "PropertyGroup";
    public const string AssemblyName = "AssemblyName";
    public const string UserSecretsId = "UserSecretsId";
    public const string EmbeddedResource = "EmbeddedResource";
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
        console.Error($"The csproj file '{csprojFile}' does not contain a Project element.");
    }

    public static void CsprojFileDoesNotContainAPropertyGroupElement(
        this IConsoleLogger console,
        string csprojFile)
    {
        console.Warning(
            $"The csproj file '{csprojFile}' does not contain a PropertyGroup element. Adding one.");
    }

    public static void EnsureEmbeddedResourceExists(
        this IConsoleLogger console,
        string csprojFile)
    {
        console.Debug($"Ensuring embedded is in the csproj file '{csprojFile}'");
    }

    public static void AddedEmbeddedResourceToTheCsprojFile(
        this IConsoleLogger console,
        string path)
    {
        console.Debug($"Added EmbeddedResource '{path}' to the csproj file");
    }

    public static void EmbeddedResourceAlreadyExistsInTheCsprojFile(
        this IConsoleLogger console,
        string path)
    {
        console.Debug($"EmbeddedResource '{path}' already exists in the csproj file");
    }
}
