using Confix.Variables;

namespace Confix.Tool.Middlewares;

public delegate IVariableResolver CreateResolver(
    string environment,
    VariableListCache variableListCache);

public sealed record VariablesFeature(
    CreateResolver CreateResolver,
    IVariableResolver Resolver,
    IVariableReplacerService Replacer,
    IVariableExtractorService Extractor);
