namespace Confix.Tool;

public static class Context
{
    public static Key<bool> DisableStatus { get; } = new("Confix.Tool.Settings.DisableStatus");

    public static Key<object> Output { get; } = new("Confix.Tool.Output");

    public static Key<bool> DisableConfigurationWrite { get; } =
        new("Confix.Tool.Settings.DisableConfigurationWrite");

    public readonly record struct Key<T>(string Id);
}
