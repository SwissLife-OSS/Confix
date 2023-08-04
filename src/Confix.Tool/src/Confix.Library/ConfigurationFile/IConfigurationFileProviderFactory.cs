using Confix.Tool.Abstractions;

namespace Confix.Tool.Middlewares;

public interface IConfigurationFileProviderFactory
{
    IConfigurationFileProvider Create(ConfigurationFileDefinition definition);
}
