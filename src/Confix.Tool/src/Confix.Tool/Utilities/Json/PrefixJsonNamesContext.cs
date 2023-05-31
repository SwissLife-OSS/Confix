namespace Confix.Tool.Schema;

public readonly struct PrefixJsonNamesContext
{
    public PrefixJsonNamesContext(string prefix)
    {
        Prefix = prefix;
    }

    public string Prefix { get; }
}
