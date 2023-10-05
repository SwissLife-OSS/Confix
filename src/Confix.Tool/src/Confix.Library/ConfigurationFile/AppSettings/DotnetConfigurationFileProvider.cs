using System.Runtime.InteropServices;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.Runtime.InteropServices.RuntimeInformation;

namespace Confix.ConfigurationFiles;

public sealed class AppSettingsConfigurationFileProvider : IConfigurationFileProvider
{
    public static string Type => "appsettings";

    public async Task<IReadOnlyList<ConfigurationFile>> GetConfigurationFilesAsync(
        IConfigurationFileContext context,
        CancellationToken ct)
    {
        var files = new List<ConfigurationFile>();

        var configuration =
            AppSettingsConfigurationFileProviderConfiguration.Parse(context.Definition.Value);

        if (context.Project.Directory?.FindInPath(FileNames.AppSettings, false) is not { } input)
        {
            // we cannot create a appsettings.json file because otherwise "component only" projects
            // will be difficult to create. 
            // We log a information though when we find a csproj file in the project directory that
            // the project will be treated as a "component only" project.
            if (context.Project.Directory is { FullName: { } fullName } d &&
                d.EnumerateFiles().Any(x => x.Extension == ".csproj"))
            {
                context.Logger.ProjectTreatedAsComponentOnly(fullName);
            }

            return Array.Empty<ConfigurationFile>();
        }

        var output = input;

        if (configuration.UseUserSecrets is true)
        {
            context.Logger.UseUserSecrets();
            var csproj = DotnetHelpers.FindProjectFileInPath(context.Project.Directory!);
            if (csproj is not null)
            {
                var userSecretsId = await DotnetHelpers.EnsureUserSecretsIdAsync(csproj, ct);
                var userSecretsFolder = GetUserSecretPath(userSecretsId);
                output = new FileInfo(Path.Combine(userSecretsFolder.FullName, FileNames.Secrets));
                context.Logger.UseUserSecretsConfigurationFile(output);
            }
        }

        context.Logger.FoundAppSettingsConfigurationFile(input);

        files.Add(new ConfigurationFile { InputFile = input, OutputFile = output });

        return files;
    }

    private static DirectoryInfo GetUserSecretPath(string userSecretsId)
        => new DirectoryInfo(
                IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(GetFolderPath(ApplicationData), "Microsoft\\UserSecrets")
                    : Path.Combine(GetFolderPath(UserProfile), ".microsoft/usersecrets"))
            .Append(userSecretsId)
            .EnsureFolder();
}

file static class FileNames
{
    public const string Secrets = "secrets.json";
    public const string AppSettings = "appsettings.json";
}

file static class Log
{
    public static void FoundAppSettingsConfigurationFile(
        this IConsoleLogger console,
        FileInfo file)
    {
        console.Debug($"Found a appsettings.json configuration file '{file}'");
    }

    public static void UseUserSecrets(this IConsoleLogger console)
    {
        console.Debug("Use user secrets");
    }

    public static void UseUserSecretsConfigurationFile(
        this IConsoleLogger console,
        FileInfo file)
    {
        console.Debug($"Use user secrets configuration file '{file}'");
    }

    public static void ProjectTreatedAsComponentOnly(
        this IConsoleLogger console,
        string projectDirectory)
    {
        console.Information(
            $"The project directory '{projectDirectory}' will be treated as a component only project as it does not contain a {FileNames.AppSettings} file.");
    }
}
