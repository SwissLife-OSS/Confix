using Confix.Tool.Schema;
using Confix.Utilities.FileSystem;

namespace Confix.Inputs;

public static class DirectoryTestExtensions
{
    public static FileInfo CreateFileInPath(
        this DirectoryInfo directory,
        string path,
        string? content = null)
    {
        var file = new FileInfo(Path.Combine(directory.FullName, path));
        file.Directory?.Create();
        if (content is not null)
        {
            File.WriteAllText(file.FullName, content);
        }

        return file;
    }

    public static FileInfo CreateConfixProject(
        this DirectoryInfo directory,
        string? content = null,
        string? path = null)
    {
        path = path ?? FileNames.ConfixProject;
        return directory.CreateFileInPath(path, content ?? "{}");
    }

    public static FileInfo CreateConfixComponent(
        this DirectoryInfo directory,
        string name,
        string? projectPath = null,
        string? content = null)
    {
        projectPath = projectPath ?? "";
        return directory.CreateFileInPath(
            Path.Combine(projectPath, FolderNames.Components, name, FileNames.ConfixComponent),
            content ?? $$""" { "name": "{{name}}" } """);
    }

    public static FileInfo CreateConfixSolution(
        this DirectoryInfo directory,
        string? content = null,
        string? path = null)
    {
        path = path ?? FileNames.ConfixSolution;
        return directory.CreateFileInPath(path, content ?? "{}");
    }

    public static FileInfo CreateConfixRc(
        this DirectoryInfo directory,
        string content,
        string? path = null)
    {
        path = path ?? FileNames.ConfixRc;
        return directory.CreateFileInPath(path, content ?? "{}");
    }

    public static void CreateConsoleApp(this DirectoryInfo directory)
    {
        directory.CreateFileInPath(
            "Program.cs",
            """
                Console.WriteLine("Hello World!");
            """);
        directory.CreateFileInPath("Confix.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
            
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>

            </Project>
            """);
    }
}
