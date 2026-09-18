using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Confix.Runner;

internal static class ConfixRuntimeResolver
{
    private static readonly string[] _owned = ["Confix.Options", "Confix.Abstractions"];

    /// <summary>
    /// The runner executes against the application's dependency graph, which pins the Confix
    /// version the application was compiled against. The runtime never rolls back to an older
    /// assembly, so the runner supplies its own copy and lets the application roll forward onto
    /// it. Registered as a module initializer because the JIT resolves these types as soon as a
    /// method referencing them is entered.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        AssemblyLoadContext.Default.Resolving += Resolve;
    }

    private static Assembly? Resolve(AssemblyLoadContext context, AssemblyName name)
    {
        if (name.Name is null || !_owned.Contains(name.Name))
        {
            return null;
        }

        var path = Path.Combine(AppContext.BaseDirectory, name.Name + ".dll");

        return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
    }
}
