namespace Confix;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConfixSectionAttribute(string path) : Attribute
{
    public string Path { get; } = path;

    public bool Required { get; set; } = true;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfixRequiredKeyAttribute : Attribute;

/// <summary>Declares configuration setup shared by the application and validation runner.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ConfixModuleAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}

/// <summary>
/// Rejects null entries in a collection or dictionary of scalars; use Required separately for
/// the collection itself. The binder materializes null object entries, so a collection of
/// objects is checked through the rules declared on the item type.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfixRequiredItemsAttribute : Attribute;
