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
        // First protect JSON escape sequences, then replace backslashes, then restore escapes.
        content = content
            .Replace("\\u", "\x00u")   // Protect \u (unicode)
            .Replace("\\n", "\x00n")   // Protect \n (newline)
            .Replace("\\r", "\x00r")   // Protect \r (carriage return)
            .Replace("\\t", "\x00t")   // Protect \t (tab)
            .Replace("\\b", "\x00b")   // Protect \b (backspace)
            .Replace("\\f", "\x00f")   // Protect \f (form feed)
            .Replace("\\\"", "\x00\"") // Protect \" (escaped quote)
            .Replace("://", "\x01")    // Protect :// (URL scheme separator)
            .Replace("\\", "/")        // Replace ALL backslashes with forward slashes
            .Replace("//", "/")        // Normalize double forward slashes to single
            .Replace("\x01", "://")    // Restore URL scheme separator
            .Replace("\x00", "\\");    // Restore all protected JSON escape sequences

        content.MatchSnapshot();
    }

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
