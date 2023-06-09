using System.Text.Json.Nodes;

namespace Confix.Tool.Schema;

public static class FileInfoExtensions
{
    public static DirectoryInfo GetDirectory(this FileInfo fileInfo)
    {
        if (File.Exists(fileInfo.FullName) && fileInfo.Directory is not null)
        {
            // If it's a file, return the directory it's contained in
            return fileInfo.Directory;
        }

        if (Directory.Exists(fileInfo.FullName))
        {
            // If it's a directory, return itself as DirectoryInfo
            return new DirectoryInfo(fileInfo.FullName);
        }

        // If it's neither, return an empty DirectoryInfo
        return new DirectoryInfo(fileInfo.FullName);
    }

    /// <summary>
    /// Deletes the file if it exists and opens a new stream to write to it.
    /// </summary>
    public static Stream OpenReplacementStream(this FileInfo fileInfo)
    {
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }

        return File.Open(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}
