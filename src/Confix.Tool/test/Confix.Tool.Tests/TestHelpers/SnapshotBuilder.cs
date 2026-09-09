using System.Text;
using System.Text.RegularExpressions;
using Snapshooter.Xunit;

namespace Confix.Inputs;

public sealed partial class SnapshotBuilder
{
    private readonly StringBuilder _builder = new();
    private readonly List<(string, string)> _replacements = new();
    private List<Func<string, string>> _processors = new();

    public SnapshotBuilder Append(string name, string content)
    {
        AddSeparator();
        _builder.AppendLine("### " + name);
        AddSeparator();
        _builder.AppendLine(content);

        return this;
    }

    public SnapshotBuilder AddReplacement(string original, string replacement)
    {
        var normalizedOriginal = original.Replace('\\', '/');

        _processors.Add(x => x.Replace(normalizedOriginal, replacement));

        var jsonEscaped = normalizedOriginal.Replace("/", "\\/");
        _processors.Add(x => x.Replace(jsonEscaped, replacement));

        return this;
    }

    private void AddSeparator()
    {
        _builder.AppendLine("--------------------------------------------------");
    }

    public void MatchSnapshot()
    {
        var content = NormalizePathSeparators(_builder.ToString());
        content = _processors
            .Aggregate(content, (current, processor) => processor(current));

        content.MatchSnapshot();
    }

    public SnapshotBuilder RemoveLineThatStartsWith(string value)
    {
        _processors.Add(
            x => string.Join(
                "\n",
                x.Split(["\r\n", "\n"], StringSplitOptions.None)
                    .Where(y => !y.StartsWith(value))));
        return this;
    }

    public SnapshotBuilder RemoveDateTimes()
    {
        _processors.Add(
            x => ReplaceDateTimeRegex()
                .Replace(x, "<<date>>"));

        return this;
    }

    public static SnapshotBuilder New() => new();

    public static string NormalizePaths(string content)
    {
        content = NormalizePathSeparators(content);
        return WindowsDriveLetterRegex().Replace(content, "/");
    }

    private static string NormalizePathSeparators(string content)
    {
        content = JsonEscapeNotInPathRegex().Replace(content, "\x00$1");
        content = content.Replace("\\", "/");
        content = content.Replace("\x00", "\\");
        return MultipleSlashesRegex().Replace(content, "/");
    }

    [GeneratedRegex(@"\\([""\/bfnrt](?![a-zA-Z0-9_])|u[0-9a-fA-F]{4})")]
    private static partial Regex JsonEscapeNotInPathRegex();

    [GeneratedRegex(@"(?<![a-zA-Z]{2,}:)/{2,}")]
    private static partial Regex MultipleSlashesRegex();

    [GeneratedRegex(@"(?<![A-Za-z])[A-Za-z]:/")]
    private static partial Regex WindowsDriveLetterRegex();

    /// <summary>
    /// we cannot match the date times fully. We can only match the date part,
    /// but still want to replace the whole string. 
    /// given
    /// { "date": "2021-09-01T00:00:00.0000000Z" } => { "date": "date" }
    /// we search for strings that contain any date but match the whole string
    /// and replace it with "date"
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"""[0-9]{4}-[0-9]{2}-[0-9]{2}T.*?""")]
    private static partial Regex ReplaceDateTimeRegex();
}
