namespace Confix.Tool.Schema;

public static class FolderExtensions
{
    public static DirectoryInfo GetVsCodeFolder(this DirectoryInfo fileInfo)
    {
        return new DirectoryInfo(Path.Combine(fileInfo.FullName, ".vscode"));
    }

    public static DirectoryInfo GetConfixFolder(this DirectoryInfo fileInfo)
    {
        return new DirectoryInfo(Path.Combine(fileInfo.FullName, ".confix"));
    }

    public static DirectoryInfo GetSchemasFolder(this DirectoryInfo fileInfo)
    {
        return new DirectoryInfo(Path.Combine(GetConfixFolder(fileInfo).FullName, ".schemas"));
    }

    public static FileInfo GetSettingsJson(this DirectoryInfo fileInfo)
    {
        return new FileInfo(Path.Combine(GetVsCodeFolder(fileInfo).FullName, "settings.json"));
    }

    public static DirectoryInfo EnsureFolder(this DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            Directory.CreateDirectory(directoryInfo.FullName);
        }

        return directoryInfo;
    }
}
