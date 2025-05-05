using Confix.Tool.Common.Pipelines;

namespace Confix.Variables;

public class VariableProviderContext : IVariableProviderContext
{
    public VariableProviderContext(
        IParameterCollection parameters,
        CancellationToken cancellationToken)
    {
        Parameters = parameters;
        CancellationToken = cancellationToken;
    }

    public IParameterCollection Parameters { get; }
    public CancellationToken CancellationToken { get; }
}