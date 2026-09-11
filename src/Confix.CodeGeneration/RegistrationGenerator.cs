using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Confix.CodeGeneration;

[Generator]
public sealed class RegistrationGenerator : IIncrementalGenerator
{
    private const string ExtensionsType = "Confix.ConfixOptionsExtensions";
    private const string ModuleAttribute = "Confix.ConfixModuleAttribute";
    private const string ModuleInterface = "Confix.IConfixModule";

    private static readonly DiagnosticDescriptor Unsupported = new("CONFIX001",
        "Shared configuration module required",
        "Registration cannot be reproduced safely: {0}. Put configuration setup in an IConfixModule and declare it with assembly: ConfixModule.",
        "Confix", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var modules = context.SyntaxProvider
            .ForAttributeWithMetadataName(ModuleAttribute,
                static (_, _) => true,
                static (syntaxContext, _) => GetModules(syntaxContext))
            .SelectMany(static (declared, _) => declared)
            .Collect();

        var registrations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsCandidate(node),
                static (syntaxContext, _) => GetRegistration(syntaxContext))
            .Where(static registration => registration is not null)
            .Select(static (registration, _) => registration!)
            .Collect();

        context.RegisterSourceOutput(modules.Combine(registrations),
            static (production, source) => Generate(production, source.Left, source.Right));
    }

    private static bool IsCandidate(SyntaxNode node)
        => node is InvocationExpressionSyntax invocation && invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => IsCandidateName(member.Name),
            SimpleNameSyntax name => IsCandidateName(name),
            _ => false
        };

    private static bool IsCandidateName(SimpleNameSyntax name)
        => name.Identifier.Text is "AddConfixOptions" or "AddConfixModule";

    private static ImmutableArray<ModuleDeclaration> GetModules(GeneratorAttributeSyntaxContext context)
    {
        var declared = ImmutableArray.CreateBuilder<ModuleDeclaration>();
        foreach (var attribute in context.Attributes)
        {
            if (attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
            {
                continue;
            }
            var location = attribute.ApplicationSyntaxReference is { } reference
                ? Location.Create(reference.SyntaxTree, reference.Span)
                : context.TargetNode.GetLocation();
            declared.Add(new ModuleDeclaration(
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                LocationInfo.From(location)));
        }
        return declared.ToImmutable();
    }

    private static Registration? GetRegistration(GeneratorSyntaxContext context)
    {
        var call = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(call).Symbol is not IMethodSymbol symbol ||
            symbol.ContainingType?.ToDisplayString() != ExtensionsType)
        {
            return null;
        }

        var location = LocationInfo.From(call.GetLocation());
        if (symbol.Name == "AddConfixModule")
        {
            return new Registration(RegistrationKind.ModuleActivation, null, null, false, location);
        }
        if (symbol.Name != "AddConfixOptions")
        {
            return null;
        }

        var owner = call.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var ownerType = owner is null
            ? null
            : context.SemanticModel.GetDeclaredSymbol(owner) as INamedTypeSymbol;
        var insideModule = ownerType is not null &&
            ownerType.AllInterfaces.Any(i => i.ToDisplayString() == ModuleInterface);

        var (statement, reason) = TryReplay(context.SemanticModel, call, symbol);
        return new Registration(RegistrationKind.Options, statement, reason, insideModule, location);
    }

    /// <summary>Only unconditional top-level standalone registrations can be replayed verbatim.</summary>
    private static (string? Statement, string? Reason) TryReplay(
        SemanticModel model, InvocationExpressionSyntax call, IMethodSymbol symbol)
    {
        if (call.Parent is not ExpressionStatementSyntax statement ||
            statement.Parent is not GlobalStatementSyntax)
        {
            return (null, "only standalone top-level calls are automatic");
        }
        if (symbol.TypeArguments.Length != 1)
        {
            return (null, "the options type must be explicit");
        }

        var arguments = new List<string>();
        var offset = symbol.IsExtensionMethod && symbol.ReducedFrom is null ? 1 : 0;
        for (var i = offset + 1; i < call.ArgumentList.Arguments.Count; i++)
        {
            var argument = call.ArgumentList.Arguments[i];
            var name = argument.NameColon?.Name.Identifier.Text ?? (i - offset == 1 ? "section" : "name");
            var constant = model.GetConstantValue(argument.Expression);
            if (!constant.HasValue || !(constant.Value is null || constant.Value is string))
            {
                return (null, "section/name must be constants");
            }
            arguments.Add(constant.Value is string text
                ? $", {name}: {SymbolDisplay.FormatLiteral(text, true)}"
                : $", {name}: null");
        }

        var optionType = symbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return ($"global::Confix.ConfixOptionsExtensions.AddConfixOptions<{optionType}>(services, configuration"
            + string.Concat(arguments) + ");", null);
    }

    private static void Generate(
        SourceProductionContext context,
        ImmutableArray<ModuleDeclaration> modules,
        ImmutableArray<Registration> registrations)
    {
        // Syntax providers do not guarantee ordering; sort so the catalog stays deterministic.
        var ordered = registrations.Sort(static (left, right) => left.Location.CompareTo(right.Location));
        var statements = new List<string>();

        if (modules.Length > 0)
        {
            foreach (var registration in ordered)
            {
                if (registration.Kind == RegistrationKind.Options && !registration.InsideModule)
                {
                    Report(context, registration.Location,
                        "all option setup must be inside shared modules when modules are declared");
                }
            }
            foreach (var module in modules.Sort(static (left, right) => left.Location.CompareTo(right.Location)))
            {
                statements.Add($"new {module.TypeName}().Configure(services, configuration);");
            }
        }
        else
        {
            foreach (var registration in ordered)
            {
                if (registration.Kind == RegistrationKind.ModuleActivation)
                {
                    Report(context, registration.Location, "module activation needs an assembly declaration");
                }
                else if (registration.Statement is { } replayed)
                {
                    statements.Add(replayed);
                }
                else
                {
                    Report(context, registration.Location, registration.Reason!);
                }
            }
        }

        // An empty catalog is intentional for declaration-only packages; the runner rejects hosts
        // with no active registrations. Only the root assembly's catalog is executed.
        context.AddSource("ConfixContractCatalog.g.cs", $@"// <auto-generated/>
namespace Confix.Generated;
public static class ContractCatalog
{{
    public static void Register(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        global::Microsoft.Extensions.Configuration.IConfiguration configuration)
    {{
        {string.Join("\n        ", statements)}
    }}
}}
");
    }

    private static void Report(SourceProductionContext context, LocationInfo location, string reason)
        => context.ReportDiagnostic(Diagnostic.Create(Unsupported, location.ToLocation(), reason));

    private enum RegistrationKind
    {
        Options,
        ModuleActivation
    }

    private sealed record Registration(
        RegistrationKind Kind,
        string? Statement,
        string? Reason,
        bool InsideModule,
        LocationInfo Location);

    private sealed record ModuleDeclaration(string TypeName, LocationInfo Location);

    /// <summary>Roslyn locations are not cacheable across incremental runs; keep the coordinates.</summary>
    private sealed record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
        : IComparable<LocationInfo>
    {
        public static LocationInfo From(Location location)
        {
            var span = location.GetLineSpan();
            return new LocationInfo(span.Path, location.SourceSpan, span.Span);
        }

        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

        public int CompareTo(LocationInfo? other)
        {
            if (other is null)
            {
                return 1;
            }
            var path = string.CompareOrdinal(FilePath, other.FilePath);
            return path != 0 ? path : TextSpan.Start.CompareTo(other.TextSpan.Start);
        }
    }
}
