using ConfiX.Entities.Component.Configuration;

namespace ConfiX.Inputs;

public static class SnapshotBuilderExtensions
{
    public static SnapshotBuilder AddOutput(this SnapshotBuilder builder, TestConfixCommandline cli)
        => builder
            .AddReplacement(cli.Directories.Content.Parent!.FullName, "<<root>>")
            .Append("CLI Output", cli.Output.ReplacePath(cli, "/tmp"));

    public static SnapshotBuilder AddFile(this SnapshotBuilder builder, string path)
        => builder.Append($"File: {path}", File.ReadAllText(path));

    public static SnapshotBuilder AddFile(this SnapshotBuilder builder, FileInfo path)
        => builder.AddFile(path.FullName);
}
