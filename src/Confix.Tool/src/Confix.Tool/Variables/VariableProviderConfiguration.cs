using System.Text.Json;

namespace ConfiX.Variables;

public record VariableProviderConfiguration
{
    public required string Name { get; init; }
    public required string Type { get; init; }

    /// <summary>
    /// Hold Overrides for the current environment
    /// </summary>
    public required JsonElement Overrides { get; init; }
};

/*
confix variables reload: This command reloads the variables for a project from the providers. Useful for updating your local environment with newly created variables.

confix variables set <variable> <value>: This command sets the value for a specified variable.

confix variables get <variable>: This command retrieves the current value of a specified variable.
*/