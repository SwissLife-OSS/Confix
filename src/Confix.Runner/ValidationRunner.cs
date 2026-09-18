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

    private static readonly TimeSpan _composeTimeout = TimeSpan.FromMinutes(2);

    private const string GenericFailure =
        "Validation runner failed. Check host composition and target runtime compatibility.";

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
        catch (Exception ex)
        {
            await WriteAsync(
                output,
                new Response(ProtocolVersion, [$"{GenericFailure} ({ex.GetType().Name})"], null));

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

        using var host = Capture(assembly, document);

        var contracts = host.Services.GetServices<IConfixContract>().ToArray();
        var strict = payload["coverage"]?.GetValue<string>() != "registeredSections";

        // Coverage and shape checks run over the candidate document alone, so environment
        // variables of the validation process are never reported as configuration.
        var errors = ContractValidation.Validate(host.Services, configuration, strict).ToList();
        CheckSectionObjects(document, contracts, errors);

        // The schema comes from the contracts alone, so scaffolding tooling can request it
        // precisely when the document is still invalid.
        var schema = payload["exportSchema"]?.GetValue<bool>() == true
            ? SchemaExport.Export(contracts, strict)
            : null;

        return (errors, schema);
    }

    private static IConfigurationRoot Build(JsonNode document)
    {
        var bytes = Encoding.UTF8.GetBytes(document.ToJsonString());

        return new ConfigurationBuilder().AddJsonStream(new MemoryStream(bytes)).Build();
    }

    /// <summary>
    /// Composes the application's real host with the candidate configuration staged as its
    /// appsettings, so the validated contracts are exactly the registrations the app performs.
    /// </summary>
    private static CapturedHost Capture(Assembly assembly, JsonNode document)
    {
        var staging = Directory.CreateTempSubdirectory("confix-validate-");
        var original = Directory.GetCurrentDirectory();

        try
        {
            File.WriteAllText(
                Path.Combine(staging.FullName, "appsettings.json"),
                document.ToJsonString());

            // Deterministic composition: production branch, no user secrets, no reload loops.
            Environment.SetEnvironmentVariable("CONFIX_VALIDATION", "true");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");

            Directory.SetCurrentDirectory(staging.FullName);

            return HostCapture.Run(assembly, _composeTimeout);
        }
        finally
        {
            Directory.SetCurrentDirectory(original);

            try
            {
                staging.Delete(recursive: true);
            }
            catch (IOException)
            {
                // The host may hold the staged file briefly; the OS temp cleanup owns leftovers.
            }
        }
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
    /// The runner already loaded the CLI's copies of the Confix runtime assemblies, so an
    /// incompatible application version would bind silently and fail later as an unexplained
    /// missing member.
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

            if (!IsCompatible(version, name.Version))
            {
                throw new RunnerException(
                    $"{name.Name} {version} in the application is not compatible with " +
                    $"{name.Version} in the Confix CLI. The application may reference an older " +
                    "minor of the same major version, never a newer one; update the CLI or " +
                    "downgrade the package.");
            }
        }
    }

    /// <summary>
    /// The CLI's copy is loaded and is backward compatible within its major version, so
    /// contracts compiled against an older minor are safe; newer APIs would be missing.
    /// </summary>
    internal static bool IsCompatible(Version? application, Version? cli)
    {
        return application is not null &&
            cli is not null &&
            application.Major == cli.Major &&
            application <= cli;
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
