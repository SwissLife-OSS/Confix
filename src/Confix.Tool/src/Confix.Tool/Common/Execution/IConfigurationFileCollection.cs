using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;

namespace Confix.Tool.Middlewares;

public interface IConfigurationFileCollection
    : IReadOnlyList<FileInfo>
{
    ConfixConfiguration? Configuration { get; }

    RepositoryConfiguration? Repository { get; }

    ProjectConfiguration? Project { get; }

    ComponentConfiguration? Component { get; }
}
