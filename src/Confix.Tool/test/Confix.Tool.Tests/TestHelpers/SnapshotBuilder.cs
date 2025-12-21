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
        // On Windows, convert the path to forward slashes since normalization happens first
        var normalizedOriginal = original.Replace('\\', '/');
        
        _processors.Add(x => x.Replace(normalizedOriginal, replacement));

        // Also handle JSON-escaped paths (forward slashes escaped as \/)
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
        var content = _builder.ToString();

        // FIRST: Normalize Windows path separators to forward slashes for consistent cross-platform snapshots.
        // This must happen BEFORE replacement processors run so they can match forward-slash paths.
        
        // Protect JSON escapes that are NOT followed by word characters (to distinguish \t from \test)
        content = JsonEscapeNotInPathRegex().Replace(content, "\x00$1");
        
        // Replace all remaining backslashes with forward slashes
        content = content.Replace("\\", "/");
        
        // Restore protected JSON escapes
        content = content.Replace("\x00", "\\");

        // Collapse multiple forward slashes to single (but preserve :// for URLs)
        content = MultipleSlashesRegex().Replace(content, "/");

        // THEN: Apply replacement processors (which now work on normalized forward-slash paths)
        content = _processors
            .Aggregate(content, (current, processor) => processor(current));

        content.MatchSnapshot();
    }

    /// <summary>
    /// Matches JSON escape sequences that are NOT followed by word characters.
    /// This distinguishes real JSON escapes (\t, \n, etc.) from paths (\test, \new, etc.)
    /// </summary>
    [GeneratedRegex(@"\\([""\/bfnrt](?![a-zA-Z0-9_])|u[0-9a-fA-F]{4})")]
    private static partial Regex JsonEscapeNotInPathRegex();

    /// <summary>
    /// Matches two or more consecutive forward slashes that should be collapsed to one.
    /// Preserves :// in URLs (https://, http://, file://) but collapses X:// for Windows drive letters.
    /// </summary>
    [GeneratedRegex(@"(?<![a-zA-Z]{2,}:)/{2,}")]
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
    /// Normalizes Windows paths in a string to Unix-style forward slashes.
    /// Use this when calling Snapshot.Match directly with content that may contain paths.
    /// </summary>
    public static string NormalizePaths(string content)
    {
        // Protect JSON escapes that are NOT followed by word characters (to distinguish \t from \test)
        content = JsonEscapeNotInPathRegex().Replace(content, "\x00$1");
        
        // Replace all remaining backslashes with forward slashes
        content = content.Replace("\\", "/");
        
        // Restore protected JSON escapes
        content = content.Replace("\x00", "\\");

        // Collapse multiple forward slashes to single (but preserve :// for URLs)
        content = MultipleSlashesRegex().Replace(content, "/");
        
        // Remove Windows drive letters (e.g., "D:/" -> "/")
        content = WindowsDriveLetterRegex().Replace(content, "/");

        return content;
    }

    /// <summary>
    /// Matches Windows drive letters like C:/ or D:/
    /// </summary>
    [GeneratedRegex(@"[A-Za-z]:/")]
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
