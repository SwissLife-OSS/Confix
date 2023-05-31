namespace ConfiX.Variables;

public interface IVariableProvider
{
    string Type { get; }

    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);
    Task<string> ResolveAsync(string path, CancellationToken cancellationToken);
    Task<string> SetAsync(string path, string value, CancellationToken cancellationToken);
}

/*
confix variables reload: This command reloads the variables for a project from the providers. Useful for updating your local environment with newly created variables.

confix variables set <variable> <value>: This command sets the value for a specified variable.

confix variables get <variable>: This command retrieves the current value of a specified variable.
*/