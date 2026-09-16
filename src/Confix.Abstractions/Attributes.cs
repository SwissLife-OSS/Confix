namespace Confix;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConfixSectionAttribute(string path) : Attribute
{
    public string Path { get; } = path;

    public bool Required { get; set; } = true;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfixRequiredKeyAttribute : Attribute;
