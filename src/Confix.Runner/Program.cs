using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix;
using Confix.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// stdout is exclusively the protocol. Application callbacks cannot corrupt it or print secrets.
var output = Console.Out;
Console.SetOut(TextWriter.Null);
Console.SetError(TextWriter.Null);
try
{
    if (args.Length != 1)
    {
        throw new InvalidOperationException();
    }
    var request = JsonNode.Parse(await Console.In.ReadToEndAsync())!.AsObject();
    if (request["version"]?.GetValue<int>() != 1)
    {
        throw new InvalidOperationException();
    }
    var assembly = LoadApplication(args[0]);
    var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(
        System.Text.Encoding.UTF8.GetBytes(request["configuration"]!.ToJsonString()))).Build();
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(config);
    var catalog = assembly.GetType("Confix.Generated.ContractCatalog")
        ?? throw new RunnerException(
            "The application has no generated Confix catalog. Reference Confix.CodeGeneration from the host project.");
    catalog.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [services, config]);
    using var provider = services.BuildServiceProvider();
    var strict = request["coverage"]?.GetValue<string>() != "registeredSections";
    var errors = ContractValidation.Validate(provider, config, strict).ToList();
    ValidateSectionObjects(request["configuration"], provider.GetServices<IConfixContract>(), errors);
    JsonObject? schema = null;
    if (errors.Count == 0 && request["exportSchema"]?.GetValue<bool>() == true)
    {
        schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict);
    }
    await output.WriteLineAsync(JsonSerializer.Serialize(new { version = 1, errors, schema }));
    return errors.Count == 0 ? 0 : 1;
}
catch (RunnerException ex)
{
    // The message is runner-authored, so it never carries application values.
    await output.WriteLineAsync(JsonSerializer.Serialize(new { version = 1, errors = new[] { ex.Message } }));
    return 2;
}
catch
{
    await output.WriteLineAsync("{\"version\":1,\"errors\":[\"Validation runner failed. Check catalog, module services, and target runtime compatibility.\"]}");
    return 2;
}

static Assembly LoadApplication(string path)
{
    var app = Path.GetFullPath(path);
    var resolver = new AssemblyDependencyResolver(app);
    EnsureCompatible(resolver);
    AssemblyLoadContext.Default.Resolving += (_, name) =>
    {
        var assemblyPath = resolver.ResolveAssemblyToPath(name);
        return assemblyPath is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
    };
    return AssemblyLoadContext.Default.LoadFromAssemblyPath(app);
}

// The runner already loaded the CLI's copies of the Confix runtime assemblies, so a different
// application version would bind silently and fail later as an unexplained missing member.
static void EnsureCompatible(AssemblyDependencyResolver resolver)
{
    foreach (var loaded in new[] { typeof(IConfixContract).Assembly, typeof(ConfixSectionAttribute).Assembly })
    {
        var name = loaded.GetName();
        var applicationPath = resolver.ResolveAssemblyToPath(name);
        if (applicationPath is null)
        {
            throw new RunnerException(
                $"The application does not reference {name.Name}. Add the package to the host project.");
        }
        var applicationVersion = AssemblyName.GetAssemblyName(applicationPath).Version;
        if (applicationVersion != name.Version)
        {
            throw new RunnerException(
                $"{name.Name} {applicationVersion} in the application does not match {name.Version} in the Confix CLI. " +
                "Align the CLI and package versions.");
        }
    }
}

// IConfiguration cannot distinguish every null section from an empty object;
// check the original JSON as well as the bound options.
static void ValidateSectionObjects(JsonNode? configuration, IEnumerable<IConfixContract> contracts, List<string> errors)
{
    foreach (var contract in contracts)
    {
        JsonNode? node = configuration;
        var present = true;
        foreach (var segment in contract.Section.Length == 0 ? Array.Empty<string>() : contract.Section.Split(':'))
        {
            if (node is not JsonObject obj)
            {
                present = false;
                break;
            }
            var property = obj.FirstOrDefault(p => p.Key.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (property.Key is null)
            {
                present = false;
                break;
            }
            node = property.Value;
        }
        if (present && node is not JsonObject)
        {
            errors.Add($"{contract.Section}: expected a nonnull configuration object.");
        }
    }
}
