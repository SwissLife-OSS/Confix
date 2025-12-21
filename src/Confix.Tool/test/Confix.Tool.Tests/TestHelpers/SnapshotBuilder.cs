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
        // Only replace backslashes that look like path separators (followed by word chars, dots, or angle brackets)
        // but NOT JSON escape sequences like \u, \n, \r, \t, \b, \f, \\, \"
        content = PathSeparatorRegex().Replace(content, "/$1");

        content.MatchSnapshot();
    }

    /// <summary>
    /// Matches backslashes followed by characters that indicate a path segment,
    /// excluding JSON/string escape sequences (\u, \n, \r, \t, \b, \f, \\, \", \/).
    /// The negative lookahead (?![ubfnrt\\""/]) ensures we don't match escape sequences.
    /// </summary>
    [GeneratedRegex(@"\\(?![ubfnrt\\""/])([A-Za-z0-9_.<>])")]
    private static partial Regex PathSeparatorRegex();

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
