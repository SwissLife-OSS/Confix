using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// stdout is exclusively the protocol. Application callbacks cannot corrupt it or print secrets.
var output = Console.Out;
Console.SetOut(TextWriter.Null);
Console.SetError(TextWriter.Null);
try
{
    if (args.Length != 1) throw new InvalidOperationException();
    var request = JsonNode.Parse(await Console.In.ReadToEndAsync())!.AsObject();
    if (request["version"]?.GetValue<int>() != 1) throw new InvalidOperationException();
    var app = Path.GetFullPath(args[0]);
    var resolver = new AssemblyDependencyResolver(app);
    AssemblyLoadContext.Default.Resolving += (_, name) =>
    {
        var path = resolver.ResolveAssemblyToPath(name);
        return path is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    };
    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(app);
    var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(
        System.Text.Encoding.UTF8.GetBytes(request["configuration"]!.ToJsonString()))).Build();
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(config);
    var catalog = assembly.GetType("Confix.Generated.ContractCatalog")
        ?? throw new InvalidOperationException("Missing catalog");
    catalog.GetMethod("Register", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [services, config]);
    using var provider = services.BuildServiceProvider();
    var errors = ContractValidation.Validate(provider, config, request["coverage"]?.GetValue<string>() != "registeredSections").ToList();
    foreach (var contract in provider.GetServices<IConfixContract>())
    {
        JsonNode? node = request["configuration"];
        var present = true;
        foreach (var segment in contract.Section.Length == 0 ? Array.Empty<string>() : contract.Section.Split(':'))
        {
            if (node is not JsonObject obj) { present = false; break; }
            var property = obj.FirstOrDefault(p => p.Key.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (property.Key is null) { present = false; break; }
            node = property.Value;
        }
        if (present && node is not JsonObject) errors.Add($"{contract.Section}: expected a nonnull configuration object.");
    }
    JsonObject? schema = null;
    if (errors.Count == 0 && request["exportSchema"]?.GetValue<bool>() == true)
        schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), request["coverage"]?.GetValue<string>() != "registeredSections");
    await output.WriteLineAsync(JsonSerializer.Serialize(new { version = 1, errors, schema }));
    return errors.Count == 0 ? 0 : 1;
}
catch
{
    await output.WriteLineAsync("{\"version\":1,\"errors\":[\"Validation runner failed. Check catalog, module services, and target runtime compatibility.\"]}");
    return 2;
}
