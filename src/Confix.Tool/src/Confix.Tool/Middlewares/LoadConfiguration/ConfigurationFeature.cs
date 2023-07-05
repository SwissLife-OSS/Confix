using Confix.Tool.Abstractions;

namespace Confix.Tool.Middlewares;

public sealed record ConfigurationFeature(
    ConfigurationScope Scope,
    IConfigurationFileCollection ConfigurationFiles,
    ProjectDefinition? Project,
    ComponentDefinition? Component,
    SolutionDefinition? Solution,
    EncryptionDefinition? Encryption);
