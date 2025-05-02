using Confix.Tool.Common.Pipelines;

namespace Confix.Variables;

public interface IVariableProviderContext
{
    /// <summary>
    /// The parameter collection.
    /// </summary>
    IParameterCollection Parameters { get; }

    /// <summary>
    /// The cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }
}