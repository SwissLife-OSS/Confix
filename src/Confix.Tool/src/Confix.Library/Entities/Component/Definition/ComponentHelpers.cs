using Confix.Tool.Entities.Components.Local;

namespace Confix.Tool.Abstractions;

public static class ComponentHelpers
{
    public static string GetKey(string provider, string componentName)
    {
        if (provider == LocalComponentProvider.Name)
        {
            return componentName;
        }

        return $"@{provider}/{componentName}";
    }
}
