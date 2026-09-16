using System.Collections.Immutable;
using Confix.CodeGeneration;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Confix.CodeFirst.Tests;

public sealed class OptionsExtensionsGeneratorTests
{
    [Fact]
    public void AnAnnotatedTypeGetsARegistrationExtension()
    {
        var result = Run("""
            [ConfixSection("Sonar")]
            public class SonarOptions { public string EnterpriseId { get; set; } = ""; }
            """);

        Generated(result.Output).Should()
            .Contain("AddSonarOptions").And
            .Contain("AddConfixOptions<global::Contracts.SonarOptions>");
    }

    [Fact]
    public void TheGeneratedCodeCompiles()
    {
        var result = Run("""
            [ConfixSection("Sonar")]
            public class SonarOptions { public string EnterpriseId { get; set; } = ""; }
            """);

        result.Diagnostics.Should().BeEmpty();
        result.Output.GetDiagnostics().Should()
            .NotContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ATypeNameWithoutTheOptionsSuffixStillReadsNaturally()
    {
        var result = Run("""
            [ConfixSection("Mail")]
            public class Mail { public string Host { get; set; } = ""; }
            """);

        Generated(result.Output).Should().Contain("AddMailOptions");
    }

    [Fact]
    public void AnInternalTypeGetsAnInternalExtension()
    {
        var result = Run("""
            [ConfixSection("Mail")]
            internal class MailOptions { public string Host { get; set; } = ""; }
            """);

        Generated(result.Output).Should()
            .Contain("internal static class MailOptionsConfixExtensions").And
            .NotContain("public static");
    }

    [Theory]
    [InlineData("""[ConfixSection("Mail")] public abstract class MailOptions { }""")]
    [InlineData("""[ConfixSection("Mail")] public class MailOptions<T> { }""")]
    public void TypesThatCannotBeBoundAreSkipped(string declaration)
    {
        var result = Run(declaration);

        result.Output.SyntaxTrees.Should()
            .NotContain(t => t.FilePath.EndsWith("ConfixOptions.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void NothingIsGeneratedWithoutConfixOptions()
    {
        var result = Run("""
            [ConfixSection("Mail")]
            public class MailOptions { public string Host { get; set; } = ""; }
            """, withOptionsPackage: false);

        result.Output.SyntaxTrees.Should()
            .NotContain(t => t.FilePath.EndsWith("ConfixOptions.g.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void HostingApplicationsGetAHostBuilderOverload()
    {
        var result = Run("""
            [ConfixSection("Sonar")]
            public class SonarOptions { public string EnterpriseId { get; set; } = ""; }
            """);

        Generated(result.Output).Should()
            .Contain("IHostApplicationBuilder builder").And
            .Contain("builder.Services, builder.Configuration");
        result.Output.GetDiagnostics().Should()
            .NotContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void LibrariesWithoutHostingOnlyGetTheServiceCollectionForm()
    {
        var result = Run("""
            [ConfixSection("Sonar")]
            public class SonarOptions { public string EnterpriseId { get; set; } = ""; }
            """, withHosting: false);

        Generated(result.Output).Should()
            .Contain("IServiceCollection services").And
            .NotContain("IHostApplicationBuilder");
        result.Output.GetDiagnostics().Should()
            .NotContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    private static string Generated(Compilation compilation)
        => compilation.SyntaxTrees
            .Single(t => t.FilePath.EndsWith("ConfixOptions.g.cs", StringComparison.Ordinal))
            .ToString();

    private static (Compilation Output, ImmutableArray<Diagnostic> Diagnostics) Run(
        string declaration,
        bool withOptionsPackage = true,
        bool withHosting = true)
    {
        var source = $$"""
            using Confix;

            namespace Contracts;

            {{declaration}}
            """;

        var platform = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        var paths = platform.Split(Path.PathSeparator)
            .Concat(Directory.GetFiles(AppContext.BaseDirectory, "Microsoft.Extensions.*.dll"))
            .Concat([
                typeof(ConfixSectionAttribute).Assembly.Location,
                typeof(ConfixOptionsExtensions).Assembly.Location
            ])
            .Distinct()
            // The test host references every assembly, so drop some to model leaner libraries.
            .Where(p => withOptionsPackage
                || !Path.GetFileName(p).Equals("Confix.Options.dll", StringComparison.Ordinal))
            .Where(p => withHosting
                || !Path.GetFileName(p).StartsWith("Microsoft.Extensions.Hosting", StringComparison.Ordinal));

        var compilation = CSharpCompilation.Create(
            "OptionsExtensionsTest",
            [CSharpSyntaxTree.ParseText(source)],
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new OptionsExtensionsGenerator().AsSourceGenerator());

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        return (output, diagnostics);
    }
}
