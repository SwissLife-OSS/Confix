using Microsoft.Extensions.DependencyInjection;

namespace Confix;

/// <summary>
/// Marks an application as a Confix consumer. Registered by the application-facing registrations,
/// never by a library describing its own section.
/// </summary>
internal sealed class ConfixApplicationMarker;

/// <summary>Records an overlap that a library description could not resolve on its own.</summary>
internal sealed record ConfixConflict(string Message);

/// <summary>Whether a description registered by a library is enforced unconditionally.</summary>
internal enum ConfixEnforcement
{
    Always,
    WhenActive
}

internal static class ConfixActivation
{
    private const string ValidationEnvironmentVariable = "CONFIX_VALIDATION";

    /// <summary>
    /// Library descriptions become enforceable while <c>confix</c> inspects the application, or
    /// once the application itself uses Confix. Applications that do not use Confix see nothing.
    /// </summary>
    internal static bool IsActive(IServiceProvider services)
    {
        return IsValidationRun() || services.GetService<ConfixApplicationMarker>() is not null;
    }

    internal static bool IsValidationRun()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable(ValidationEnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }
}
