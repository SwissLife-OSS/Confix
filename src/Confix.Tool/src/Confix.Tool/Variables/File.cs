namespace ConfiX.Variables;

// TODO figure out a name
public class VariableProviderProviderOderSo
{
    private readonly Dictionary<string, IVariableProvider> _providers;
    private readonly Dictionary<string, string> _providerNameToTypeMapping;


    public async Task<IReadOnlyDictionary<string, string>> ResolveVariables(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> resolvedVariables = new();

        // TODO: that probably need to run in paralell. or allow to fetch multiple variables from a provider at once... not quite sure
        foreach (string key in keys)
        {
            VariablePath variablePath = VariablePath.FromString(key);
            IVariableProvider provider = GetProvider(variablePath.ProviderName);
            var resolved = await provider.ResolveAsync(variablePath.Path, cancellationToken);
            resolvedVariables.Add(key, resolved);
        }

        return resolvedVariables;
    }

    private string GetProviderType(string providerName)
    {
        if (_providerNameToTypeMapping.TryGetValue(providerName, out string? providerType))
        {
            return providerType;
        }
        // TODO: custom exception
        throw new InvalidOperationException("unknown providername");
    }

    private IVariableProvider GetProvider(string providerName)
    {
        string providerType = GetProviderType(providerName);

        // TODO: somehow pass the configuration from confixrc to provider with the given name 
        // (provider type might have multiple names with different config)
        return _providers[providerType];
    }
}

public record struct VariablePath(string ProviderName, string Path)
{
    // TODO: might move to a VariableParser or similar (together with the logic of extracting variables from a json)
    public static VariablePath FromString(string s)
    {
        // TODO implement
        return new VariablePath("foo", "bar");
    }
}

/*
confix variables reload: This command reloads the variables for a project from the providers. Useful for updating your local environment with newly created variables.

confix variables set <variable> <value>: This command sets the value for a specified variable.

confix variables get <variable>: This command retrieves the current value of a specified variable.
*/