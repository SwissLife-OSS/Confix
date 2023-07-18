using ConfiX.Variables;

namespace Confix.Tool.Middlewares;

public delegate IVariableResolver CreateResolver(string environment);

public sealed record VariableResolverFeature(
    CreateResolver CreateResolver,
    IVariableResolver Resolver,
    IVariableReplacerService Replacer);
