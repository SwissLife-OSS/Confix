using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents a collection of parameters that are passed along the pipeline. These parameters can
/// be <see cref="Option"/> or <see cref="Argument"/>.
/// The parameter collection allows data transfer between command and  pipeline. 
/// </summary>
public interface IParameterCollection
{
    /// <summary>
    /// Retrieves a parameter of the given type associated with the provided key.
    /// </summary>
    /// <typeparam name="T">The type of the parameter to retrieve.</typeparam>
    /// <param name="key">The key associated with the parameter.</param>
    /// <returns>The value of the parameter if it exists.</returns>
    public T Get<T>(string key);

    /// <summary>
    /// Tries to retrieve a parameter of the given type associated with the provided key. Does not
    /// throw an exception if the parameter doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the parameter to retrieve.</typeparam>
    /// <param name="key">The key associated with the parameter.</param>
    /// <param name="value">Out parameter to receive the value of the parameter if it exists.</param>
    /// <returns>True if the parameter exists, false otherwise.</returns>
    public bool TryGet<T>(string key, out T value);

    /// <summary>
    /// Tries to retrieve a parameter associated with the provided symbol. Does not throw an
    /// exception if the parameter doesn't exist.
    /// </summary>
    /// <param name="symbol">The symbol associated with the parameter.</param>
    /// <param name="value">Out parameter to receive the value of the parameter if it exists.</param>
    /// <returns>True if the parameter exists, false otherwise.</returns>
    public bool TryGet(Symbol symbol, out object? value);
}
