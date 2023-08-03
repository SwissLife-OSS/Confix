using System.Text;
using Snapshooter.Xunit;

namespace Confix.Inputs;

public sealed class SnapshotBuilder
{
    private readonly StringBuilder _builder = new();
    private readonly List<(string, string)> _replacements = new();

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
        _replacements.Add((original, replacement));
        return this;
    }

    private void AddSeparator()
    {
        _builder.AppendLine("--------------------------------------------------");
    }

    public void MatchSnapshot()
    {
        var content = _builder.ToString();
        foreach (var (original, replacement) in _replacements)
        {
            content = content.Replace(original, replacement);
        }

        content.MatchSnapshot();
    }

    public static SnapshotBuilder New() => new();
}