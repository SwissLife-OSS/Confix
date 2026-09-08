using System.Runtime.CompilerServices;
using Confix.Entities.Schema;

namespace Confix.Tool.Tests;

/// <summary>
/// Ensures Confix schema customizations (such as the <c>metadata</c> keyword) are registered
/// before any test runs. Confix.Library registers them from its own module initializer, but that
/// only runs once the assembly is loaded, which is not guaranteed for tests that only reference
/// constants.
/// </summary>
internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        MetadataKeyword.Register();
    }
}
