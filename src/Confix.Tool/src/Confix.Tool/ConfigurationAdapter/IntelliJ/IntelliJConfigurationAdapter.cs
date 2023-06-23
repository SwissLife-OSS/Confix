using System.Xml.Linq;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using static System.Xml.Linq.LoadOptions;

namespace Confix.Tool.Middlewares;

public sealed class IntelliJConfigurationAdapter : IConfigurationAdapter
{
    /// <inheritdoc />
    public async Task UpdateJsonSchemasAsync(IConfigurationAdapterContext context)
    {
        var settingsFile = context.SolutionRoot.GetJsonSchemasXml();
        if (!settingsFile.Directory!.Exists)
        {
            context.Logger
                .SkippingIntellijJSettingsFileAsThereIsNoIdeasFolder(settingsFile.Directory!);
            return;
        }

        var document =
            await LoadDocumentAsync(context.Logger, settingsFile, context.CancellationToken);

        context.Logger.UpdatingIntellijJSettingsFile(settingsFile.FullName);
        context.Schemas.ForEach(document.UpsertIdeaJsonSchema);

        context.Logger.SavingIntellijJSettingsFile(settingsFile.FullName);
        await using var writeStream = settingsFile.OpenReplacementStream();
        await document.SaveAsync(writeStream, SaveOptions.None, context.CancellationToken);

        context.Logger.IntellijJSettingsFileUpdated(settingsFile.FullName);
    }

    private static async Task<XDocument> LoadDocumentAsync(
        IConsoleLogger logger,
        FileInfo settingsFile,
        CancellationToken cancellationToken)
    {
        if (settingsFile.Exists)
        {
            logger.LoadingIntellijJSettingsFile(settingsFile.FullName);

            await using var readStream = settingsFile.Open(FileMode.OpenOrCreate);

            return await XDocument
                .LoadAsync(readStream, PreserveWhitespace, cancellationToken);
        }

        logger.CreatingIntellijJSettingsFile(settingsFile.FullName);

        return XDocument.Parse("""
            <project version="4">
              <component name="JsonSchemaMappingsProjectConfiguration">
                <state>
                  <map>
                  </map>
                </state>
              </component>
            </project>
        """);
    }
}

file static class FolderExtensions
{
    public static DirectoryInfo GetIdeaFolder(this FileSystemInfo fileInfo)
    {
        var directoryInfo = new DirectoryInfo(Path.Combine(fileInfo.FullName, ".idea"));
        return directoryInfo.FindInPath("workspace.xml")?.Directory ?? directoryInfo;
    }

    public static FileInfo GetJsonSchemasXml(this FileSystemInfo fileInfo)
    {
        return new FileInfo(Path.Combine(GetIdeaFolder(fileInfo).FullName, "jsonSchemas.xml"));
    }
}

file static class Logs
{
    public static void SkippingIntellijJSettingsFileAsThereIsNoIdeasFolder(
        this IConsoleLogger log,
        FileSystemInfo ideaFolder)
        => log.Debug(
            $"Skipping IntelliJ IDEA settings file as there is no .idea folder in the solution root. Expected location: {ideaFolder.FullName}");

    public static void UpdatingIntellijJSettingsFile(
        this IConsoleLogger log,
        string settingsFile)
        => log.Debug($"Updating IntelliJ IDEA settings file: {settingsFile}");

    public static void SavingIntellijJSettingsFile(
        this IConsoleLogger log,
        string settingsFile)
        => log.Debug($"Saving IntelliJ IDEA settings file: {settingsFile}");

    public static void LoadingIntellijJSettingsFile(
        this IConsoleLogger log,
        string settingsFile)
        => log.Debug($"Loading IntelliJ IDEA settings file: {settingsFile}");

    public static void CreatingIntellijJSettingsFile(
        this IConsoleLogger log,
        string settingsFile)
        => log.Debug($"Creating IntelliJ IDEA settings file: {settingsFile}");

    public static void IntellijJSettingsFileUpdated(
        this IConsoleLogger log,
        string settingsFile)
        => log.Success($"IntelliJ IDEA settings file updated: {settingsFile}");
}
