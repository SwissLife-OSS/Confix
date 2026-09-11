using Confix.CodeGeneration;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Confix.CodeFirst.Tests;

public sealed class GeneratorTests
{
    [Theory]
    [InlineData("if (true) services.AddConfixOptions<Mail>(configuration);")]
    [InlineData("services.AddConfixOptions<Mail>(configuration).Validate(x => true);")]
    public void UnsupportedActivationProducesAnError(string registration)
    {
        var result = Run(registration);
        result.Diagnostics.Should().Contain(d => d.Id == "CONFIX001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void StandaloneRegistrationProducesCompilableCatalog()
    {
        var result = Run("services.AddConfixOptions<Mail>(configuration, name: \"primary\");");
        result.Diagnostics.Should().BeEmpty();
        result.Output.GetDiagnostics().Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.Output.SyntaxTrees.Last().ToString().Should().Contain("name: \"primary\"");
    }

    private static (Compilation Output, System.Collections.Immutable.ImmutableArray<Diagnostic> Diagnostics) Run(string registration)
    {
        var source = $$"""
            using Confix;
            using Microsoft.Extensions.Configuration;
            using Microsoft.Extensions.DependencyInjection;
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();
            {{registration}}
            [ConfixSection("Mail")]
            public class Mail { public string Host { get; set; } = ""; }
            """;
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Concat(Directory.GetFiles(AppContext.BaseDirectory, "Microsoft.Extensions.*.dll"))
            .Concat([typeof(ConfixSectionAttribute).Assembly.Location, typeof(ConfixOptionsExtensions).Assembly.Location])
            .Distinct();
        var compilation = CSharpCompilation.Create("CatalogTest", [CSharpSyntaxTree.ParseText(source)],
            paths.Select(p => MetadataReference.CreateFromFile(p)), new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new RegistrationGenerator().AsSourceGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        return (output, diagnostics);
    }
}
