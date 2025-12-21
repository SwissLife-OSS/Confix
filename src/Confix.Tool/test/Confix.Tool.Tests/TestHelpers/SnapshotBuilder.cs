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
        _processors.Add(x => x.Replace(original, replacement));

        // On Windows, also replace the path with forward slashes (in case output uses normalized paths)
        if (Path.DirectorySeparatorChar == '\\' && original.Contains('\\'))
        {
            var withForwardSlashes = original.Replace('\\', '/');
            _processors.Add(x => x.Replace(withForwardSlashes, replacement));

            // Also handle JSON-escaped paths (forward slashes escaped as \/)
            var jsonEscaped = withForwardSlashes.Replace("/", "\\/");
            _processors.Add(x => x.Replace(jsonEscaped, replacement));
        }

        return this;
    }

    private void AddSeparator()
    {
        _builder.AppendLine("--------------------------------------------------");
    }

    public void MatchSnapshot()
    {
        var content = _builder.ToString();
        content = _processors
            .Aggregate(content, (current, processor) => processor(current));

        // Normalize Windows path separators to forward slashes for consistent cross-platform snapshots.
        // Use regex to protect JSON escape sequences (which are NOT followed by word characters),
        // then replace all backslashes, then restore the protected sequences.
        
        // Protect JSON escapes that are NOT followed by word characters (to distinguish \t from \test)
        content = JsonEscapeNotInPathRegex().Replace(content, "\x00$1");
        
        // Replace all remaining backslashes with forward slashes
        content = content.Replace("\\", "/");
        
        // Restore protected JSON escapes
        content = content.Replace("\x00", "\\");

        // Collapse multiple forward slashes to single (but preserve :// for URLs)
        content = MultipleSlashesRegex().Replace(content, "/");

        content.MatchSnapshot();
    }

    /// <summary>
    /// Matches JSON escape sequences that are NOT followed by word characters.
    /// This distinguishes real JSON escapes (\t, \n, etc.) from paths (\test, \new, etc.)
    /// </summary>
    [GeneratedRegex(@"\\([""\/bfnrt](?![a-zA-Z0-9_])|u[0-9a-fA-F]{4})")]
    private static partial Regex JsonEscapeNotInPathRegex();

    /// <summary>
    /// Matches two or more consecutive forward slashes that are NOT preceded by a colon (to preserve URLs like https://).
    /// </summary>
    [GeneratedRegex(@"(?<!:)/{2,}")]
    private static partial Regex MultipleSlashesRegex();

    public SnapshotBuilder RemoveLineThatStartsWith(string value)
    {
        _processors.Add(
            x => x.Split(Environment.NewLine)
                .Where(y => !y.StartsWith(value))
                .Aggregate((a, b) => a + Environment.NewLine + b));
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
