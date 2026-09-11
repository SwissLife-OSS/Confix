using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Runner;

/// <summary>
/// Executes the application's registered option contracts against a configuration document
/// supplied over stdin, using the application's own runtime and dependencies.
/// </summary>
internal static class ValidationRunner
{
    private const int ProtocolVersion = 1;
    private const string CatalogTypeName = "Confix.Generated.ContractCatalog";

    private const string GenericFailure =
        "Validation runner failed. Check catalog, module services, and target runtime compatibility.";

    private static readonly JsonSerializerOptions _responseFormat =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<int> RunAsync(string[] args, string request, TextWriter output)
    {
        try
        {
            var (errors, schema) = Validate(args, request);

            await WriteAsync(output, new Response(ProtocolVersion, errors, schema));

            return errors.Count == 0 ? 0 : 1;
        }
        catch (RunnerException ex)
        {
            // The message is runner-authored, so it never carries application values.
            await WriteAsync(output, new Response(ProtocolVersion, [ex.Message], null));

            return 2;
        }
        catch
        {
            await WriteAsync(output, new Response(ProtocolVersion, [GenericFailure], null));

            return 2;
        }
    }

    private static (List<string> Errors, JsonObject? Schema) Validate(string[] args, string request)
    {
        if (args.Length != 1)
        {
            throw new RunnerException("The validation runner expects a single assembly path.");
        }

        var payload = JsonNode.Parse(request)?.AsObject()
            ?? throw new RunnerException("The validation request was not a JSON object.");

        if (payload["version"]?.GetValue<int>() != ProtocolVersion)
        {
            throw new RunnerException("Unsupported validation request protocol.");
        }

        var document = payload["configuration"]
            ?? throw new RunnerException("The validation request carried no configuration.");

        var assembly = LoadApplication(args[0]);
        var configuration = Build(document);

        using var provider = CreateProvider(assembly, configuration);

        var contracts = provider.GetServices<IConfixContract>().ToArray();
        var strict = payload["coverage"]?.GetValue<string>() != "registeredSections";

        var errors = ContractValidation.Validate(provider, configuration, strict).ToList();
        CheckSectionObjects(document, contracts, errors);

        var schema = errors.Count == 0 && payload["exportSchema"]?.GetValue<bool>() == true
            ? SchemaExport.Export(contracts, strict)
            : null;

        return (errors, schema);
    }

    private static IConfigurationRoot Build(JsonNode document)
    {
        var bytes = Encoding.UTF8.GetBytes(document.ToJsonString());

        return new ConfigurationBuilder().AddJsonStream(new MemoryStream(bytes)).Build();
    }

    private static ServiceProvider CreateProvider(Assembly assembly, IConfiguration configuration)
    {
        var catalog = assembly.GetType(CatalogTypeName)
            ?? throw new RunnerException(
                "The application has no generated Confix catalog. " +
                "Reference Confix.CodeGeneration from the host project.");

        var register = catalog.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)
            ?? throw new RunnerException("The generated Confix catalog is malformed.");

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        register.Invoke(null, [services, configuration]);

        return services.BuildServiceProvider();
    }

    private static Assembly LoadApplication(string path)
    {
        var application = Path.GetFullPath(path);
        var resolver = new AssemblyDependencyResolver(application);

        EnsureCompatible(resolver);

        AssemblyLoadContext.Default.Resolving += (_, name) =>
        {
            var resolved = resolver.ResolveAssemblyToPath(name);

            return resolved is null
                ? null
                : AssemblyLoadContext.Default.LoadFromAssemblyPath(resolved);
        };

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(application);
    }

    /// <summary>
    /// The runner already loaded the CLI's copies of the Confix runtime assemblies, so a different
    /// application version would bind silently and fail later as an unexplained missing member.
    /// </summary>
    private static void EnsureCompatible(AssemblyDependencyResolver resolver)
    {
        Assembly[] loaded =
        [
            typeof(IConfixContract).Assembly,
            typeof(ConfixSectionAttribute).Assembly
        ];

        foreach (var name in loaded.Select(assembly => assembly.GetName()))
        {
            var path = resolver.ResolveAssemblyToPath(name)
                ?? throw new RunnerException(
                    $"The application does not reference {name.Name}. " +
                    "Add the package to the host project.");

            var version = AssemblyName.GetAssemblyName(path).Version;

            if (version != name.Version)
            {
                throw new RunnerException(
                    $"{name.Name} {version} in the application does not match " +
                    $"{name.Version} in the Confix CLI. Align the CLI and package versions.");
            }
        }
    }

    /// <summary>
    /// IConfiguration cannot distinguish every null section from an empty object, so the original
    /// JSON document is checked as well as the bound options.
    /// </summary>
    private static void CheckSectionObjects(
        JsonNode document,
        IEnumerable<IConfixContract> contracts,
        List<string> errors)
    {
        foreach (var contract in contracts)
        {
            var (found, node) = Resolve(document, contract.Section);

            if (found && node is not JsonObject)
            {
                errors.Add($"{contract.Section}: expected a nonnull configuration object.");
            }
        }
    }

    private static (bool Found, JsonNode? Node) Resolve(JsonNode document, string section)
    {
        JsonNode? node = document;

        if (section.Length == 0)
        {
            return (true, node);
        }

        foreach (var segment in section.Split(':'))
        {
            if (node is not JsonObject obj)
            {
                return (false, null);
            }

            var property = obj
                .FirstOrDefault(p => p.Key.Equals(segment, StringComparison.OrdinalIgnoreCase));

            if (property.Key is null)
            {
                return (false, null);
            }

            node = property.Value;
        }

        return (true, node);
    }

    private static async Task WriteAsync(TextWriter output, Response response)
    {
        await output.WriteLineAsync(JsonSerializer.Serialize(response, _responseFormat));
    }

    private sealed record Response(
        int Version,
        IReadOnlyList<string> Errors,
        JsonObject? Schema);
}
