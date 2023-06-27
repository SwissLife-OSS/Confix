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

    public IReadOnlyList<ConfigurationFile> GetConfigurationFiles(IConfigurationFileContext context)
    {
        var files = new List<ConfigurationFile>();

        var configuration =
            AppSettingsConfigurationFileProviderConfiguration.Parse(context.Definition.Value);

        var input = context.Project.Directory!.FindInPath(FileNames.AppSettings, false);
        
        if (input is null)
        {
            return Array.Empty<ConfigurationFile>();
        }

        var output = input;

        if (configuration.UseUserSecrets is true)
        {
            App.Log.UseUserSecrets();
            var csproj = DotnetHelpers.FindProjectFileInPath(context.Project.Directory!);
            if (csproj is not null)
            {
                var userSecretsId = DotnetHelpers.EnsureUserSecretsId(csproj);
                var userSecretsFolder = GetUserSecretPath(userSecretsId);
                output = new FileInfo(Path.Combine(userSecretsFolder.FullName, FileNames.Secrets));
                App.Log.UseUserSecretsConfigurationFile(output);
            }
        }

        App.Log.FoundAAppSettingsConfigurationFile(input);

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
    public static void FoundAAppSettingsConfigurationFile(
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
