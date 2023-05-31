using Confix.Tool;

namespace ConfiX.Variables;

// TODO figure out a name
public class VariableProviderProviderOderSo
{
    private readonly IVariableProviderFactory _variableProviderFactory;

    public async Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        IReadOnlyList<VariableProviderConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        Dictionary<VariablePath, string> resolvedVariables = new();

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {
            VariableProviderConfiguration configuration = GetProviderConfiguration(configurations, group.Key);
            IVariableProvider provider = _variableProviderFactory.CreateProvider(
                configuration.Type,
                configuration.Configuration);

            var resolved = await provider.ResolveManyAsync(group.Select(x => x.Path).ToArray(), cancellationToken);

            resolved.ForEach((r) => 
                resolvedVariables.Add(
                    group.First(i => i.Path == r.Key), 
                    r.Value));
        }

        return resolvedVariables;
    }

    private VariableProviderConfiguration GetProviderConfiguration(
        IReadOnlyList<VariableProviderConfiguration> configurations,
        string providerName)
    {
        VariableProviderConfiguration? config = configurations.FirstOrDefault(c => c.Name.Equals(providerName));
        if (config is null)
        {
            // TODO custom exception
            throw new InvalidOperationException("");
        }
        return config;
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