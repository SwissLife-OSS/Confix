using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Confix.CodeGeneration;

[Generator]
public sealed class RegistrationGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor Unsupported = new("CONFIX001", "Shared configuration module required",
        "Registration cannot be reproduced safely: {0}. Put configuration setup in an IConfixModule and declare it with assembly: ConfixModule",
        "Confix", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, Generate);
    }

    private static void Generate(SourceProductionContext context, Compilation compilation)
    {
        var modules = compilation.Assembly.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == "Confix.ConfixModuleAttribute").ToArray();
        var statements = new List<string>();
        if (modules.Length > 0)
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol symbol || symbol.Name != "AddConfixOptions" ||
                        symbol.ContainingType.ToDisplayString() != "Confix.ConfixOptionsExtensions") continue;
                    var owner = call.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var type = owner is null ? null : model.GetDeclaredSymbol(owner) as INamedTypeSymbol;
                    if (type is null || !type.AllInterfaces.Any(i => i.ToDisplayString() == "Confix.IConfixModule"))
                        context.ReportDiagnostic(Diagnostic.Create(Unsupported, call.GetLocation(), "all option setup must be inside shared modules when modules are declared"));
                }
            }
            foreach (var module in modules)
            {
                if (module.ConstructorArguments[0].Value is not INamedTypeSymbol type) continue;
                statements.Add($"new {type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}().Configure(services, configuration);");
            }
        }
        else
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol symbol ||
                        symbol.ContainingType.ToDisplayString() != "Confix.ConfixOptionsExtensions") continue;
                    if (symbol.Name == "AddConfixModule")
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Unsupported, call.GetLocation(), "module activation needs an assembly declaration"));
                        continue;
                    }
                    if (symbol.Name != "AddConfixOptions") continue;
                    // Only unconditional top-level standalone registrations are automatically replayed.
                    // Conditional/module/library wrappers need an explicit shared module.
                    if (call.Parent is not ExpressionStatementSyntax statement || statement.Parent is not GlobalStatementSyntax)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Unsupported, call.GetLocation(), "only standalone top-level calls are automatic"));
                        continue;
                    }
                    var args = new Dictionary<string, string>();
                    var valid = true;
                    var offset = symbol.IsExtensionMethod && symbol.ReducedFrom is null ? 1 : 0;
                    for (var i = offset + 1; i < call.ArgumentList.Arguments.Count; i++)
                    {
                        var arg = call.ArgumentList.Arguments[i];
                        var name = arg.NameColon?.Name.Identifier.Text ?? (i - offset == 1 ? "section" : "name");
                        var constant = model.GetConstantValue(arg.Expression);
                        if (!constant.HasValue || constant.Value is not (null or string)) { valid = false; break; }
                        args[name] = constant.Value is string text ? Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(text, true) : "null";
                    }
                    if (!valid)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Unsupported, call.GetLocation(), "section/name must be constants"));
                        continue;
                    }
                    var optionType = symbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    statements.Add($"global::Confix.ConfixOptionsExtensions.AddConfixOptions<{optionType}>(services, configuration" +
                        string.Concat(args.Select(a => $", {a.Key}: {a.Value}")) + ");");
                }
            }
        }
        // An empty catalog is intentional for declaration-only packages; the runner rejects hosts
        // with no active registrations. Only the root assembly's catalog is executed.
        context.AddSource("ConfixContractCatalog.g.cs", $$"""
            // <auto-generated/>
            namespace Confix.Generated;
            public static class ContractCatalog
            {
                public static void Register(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
                    global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                {
                    {{string.Join("\n", statements)}}
                }
            }
            """);
    }
}
