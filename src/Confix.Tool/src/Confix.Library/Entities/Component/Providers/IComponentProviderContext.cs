using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Confix.Tool.Entities.Components;

public interface IComponentProviderContext
{
    ProjectDefinition Project { get; }

    SolutionDefinition Solution { get; }

    /// <inheritdoc cref="Logger{T}"/>>
    IConsoleLogger Logger { get; }

    /// <inheritdoc cref="System.Threading.CancellationToken"/>>
    CancellationToken CancellationToken { get; }

    IList<Component> Components { get; }

    IReadOnlyList<ComponentReferenceDefinition> ComponentReferences { get; }

    IParameterCollection Parameter { get; }
}
