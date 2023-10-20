namespace Confix.Tool.Reporting;

public interface IDependencyProviderFactory
{
    IDependencyProvider Create(DependencyProviderDefinition definition);
}
