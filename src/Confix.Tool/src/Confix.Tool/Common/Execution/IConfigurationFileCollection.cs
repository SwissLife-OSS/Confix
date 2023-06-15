using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

/// <summary>
/// <p>
/// Represents a collection of configuration files with different scopes, each file being a .confix
/// file relevant to the current execution context. The interface provides access to the runtime,
/// solution, project, and component configurations which are the merged configurations of their
/// respective .confix files.
/// </p>
/// <p>
/// The naming follows the common Unix .*rc pattern, hence <see cref="FileNames.ConfixRc"/> for
/// runtime configuration. The configurations represent merged views of their respective scopes,
/// merging from the .confix files found in the execution context, up to the ones found in the home
/// directory.
/// </p>
/// </summary>
public interface IConfigurationFileCollection : IReadOnlyList<FileInfo>
{
    /// <summary>
    /// Gets the runtime configuration file. This is the merged configuration of all
    /// <see cref="FileNames.ConfixRc"/> files found in the current execution context, plus the
    /// <see cref="FileNames.ConfixRc"/> file in the home directory.
    /// </summary>
    RuntimeConfiguration? RuntimeConfiguration { get; }

    /// <summary>
    /// Gets the solution configuration file. This is the merged .confix.solution file, if
    /// available, including related solution settings from the
    /// <see cref="FileNames.ConfixRc"/> file.
    /// </summary>
    SolutionConfiguration? Solution { get; }

    /// <summary>
    /// Gets the project configuration file. This is the merged
    /// <see cref="FileNames.ConfixProject"/> file, if available, including related project settings
    /// from the <see cref="FileNames.ConfixRc"/> file and <see cref="FileNames.ConfixSolution"/>
    /// configuration.
    /// </summary>
    ProjectConfiguration? Project { get; }

    /// <summary>
    /// Gets the component configuration file. This is the .confix.component file of the current
    /// component, if available, including related settings from the
    /// <see cref="FileNames.ConfixRc"/> file, project, and solution configurations.
    /// </summary>
    ComponentConfiguration? Component { get; }
}
