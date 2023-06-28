using Confix.Variables;

namespace Confix.Tool.Middlewares;

public sealed record VariableResolverFeature(
    IVariableResolver Resolver,
    IVariableReplacerService Replacer);
