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
            // we create a appsettings.json file if we can find a csproj file. This will avoid
            // the need to create a appsettings.json file in the project directory manually. 
            if (context.Project.Directory is { FullName: { } fullName } d &&
                d.EnumerateFiles().FirstOrDefault(x => x.Extension == ".csproj") is not null)
            {
                input = new FileInfo(Path.Combine(fullName, FileNames.AppSettings));
                await File.WriteAllTextAsync(input.FullName, "{}", ct);
            }
            else
            {
                return Array.Empty<ConfigurationFile>();
            }
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
}
