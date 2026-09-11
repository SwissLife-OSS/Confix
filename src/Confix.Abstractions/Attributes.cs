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
/// Requires each collection item to be nonnull; use Required separately for the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfixRequiredItemsAttribute : Attribute;
