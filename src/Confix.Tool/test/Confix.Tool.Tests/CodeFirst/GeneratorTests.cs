using System.Collections.Immutable;
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
    [InlineData("foreach (var i in new[]{1}) services.AddConfixOptions<Mail>(configuration);")]
    [InlineData("var b = services.AddConfixOptions<Mail>(configuration);")]
    [InlineData("Register(services, configuration); static void Register(IServiceCollection services, IConfiguration configuration) => services.AddConfixOptions<Mail>(configuration);")]
    public void UnsupportedActivationProducesAnError(string registration)
    {
        var result = Run(registration);

        result.Diagnostics.Should()
            .Contain(d => d.Id == "CONFIX001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NonConstantSectionArgumentsAreRejected()
    {
        var result = Run("""
            var section = System.Environment.GetEnvironmentVariable("X")!;
            services.AddConfixOptions<Mail>(configuration, section);
            """);

        result.Diagnostics.Should()
            .ContainSingle().Which.GetMessage().Should().Contain("must be constants");
    }

    [Fact]
    public void ConstantSectionArgumentsAreReplayed()
    {
        var result = Run("""
            const string Section = "Mail";
            services.AddConfixOptions<Mail>(configuration, Section, "primary");
            """);

        result.Diagnostics.Should().BeEmpty();
        Catalog(result.Output).Should().Contain("section: \"Mail\"").And.Contain("name: \"primary\"");
    }

    [Fact]
    public void TheStaticInvocationFormIsAlsoReplayed()
    {
        var result = Run(
            "ConfixOptionsExtensions.AddConfixOptions<Mail>(services, configuration, \"Mail\");");

        result.Diagnostics.Should().BeEmpty();
        Catalog(result.Output).Should().Contain("section: \"Mail\"");
    }

    [Fact]
    public void ModuleActivationWithoutAnAssemblyDeclarationIsRejected()
    {
        var result = Run("""
            services.AddConfixModule<Setup>(configuration);
            public sealed class Setup : IConfixModule
            {
                public void Configure(IServiceCollection services, IConfiguration configuration) { }
            }
            """);

        result.Diagnostics.Should()
            .ContainSingle().Which.GetMessage().Should().Contain("assembly declaration");
    }

    [Fact]
    public void DeclaringModulesForbidsRegistrationOutsideThem()
    {
        var result = Run("""
            services.AddConfixOptions<Mail>(configuration);
            public sealed class Setup : IConfixModule
            {
                public void Configure(IServiceCollection services, IConfiguration configuration) { }
            }
            """, "[assembly: ConfixModule(typeof(Setup))]");

        result.Diagnostics.Should()
            .ContainSingle().Which.GetMessage().Should().Contain("inside shared modules");
    }

    [Fact]
    public void ProjectsWithoutAnyRegistrationStillGetAnEmptyCatalog()
    {
        var result = Run("");

        result.Diagnostics.Should().BeEmpty();
        Catalog(result.Output).Should().Contain("ContractCatalog").And.Contain("Register");
        result.Output.GetDiagnostics().Should()
            .NotContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CatalogStatementsFollowSourceOrder()
    {
        var result = Run("""
            services.AddConfixOptions<Mail>(configuration, "First", "a");
            services.AddConfixOptions<Mail>(configuration, "Second", "b");
            """);

        var catalog = Catalog(result.Output);

        catalog.IndexOf("\"First\"", StringComparison.Ordinal).Should()
            .BeLessThan(catalog.IndexOf("\"Second\"", StringComparison.Ordinal));
    }

    [Fact]
    public void StandaloneRegistrationProducesCompilableCatalog()
    {
        var result = Run("services.AddConfixOptions<Mail>(configuration, name: \"primary\");");

        result.Diagnostics.Should().BeEmpty();
        result.Output.GetDiagnostics().Should()
            .NotContain(d => d.Severity == DiagnosticSeverity.Error);
        Catalog(result.Output).Should().Contain("name: \"primary\"");
    }

    private static string Catalog(Compilation compilation)
    {
        return compilation.SyntaxTrees
            .First(t => t.FilePath.EndsWith("ConfixContractCatalog.g.cs", StringComparison.Ordinal))
            .ToString();
    }

    [Fact]
    public void ModuleDeclarationsReplaceDirectRegistrations()
    {
        var result = Run("""
            services.AddConfixModule<Setup>(configuration);
            public sealed class Setup : IConfixModule
            {
                public void Configure(IServiceCollection services, IConfiguration configuration)
                    => services.AddConfixOptions<Mail>(configuration);
            }
            """, "[assembly: ConfixModule(typeof(Setup))]");

        result.Diagnostics.Should().BeEmpty();
        result.Output.SyntaxTrees.Last().ToString().Should()
            .Contain("new global::Setup().Configure(services, configuration);");
    }

    [Fact]
    public void EveryDeclaredModuleIsActivated()
    {
        var result = Run("""
            public sealed class First : IConfixModule
            {
                public void Configure(IServiceCollection services, IConfiguration configuration)
                    => services.AddConfixOptions<Mail>(configuration);
            }
            public sealed class Second : IConfixModule
            {
                public void Configure(IServiceCollection services, IConfiguration configuration) { }
            }
            """, "[assembly: ConfixModule(typeof(First))]\n[assembly: ConfixModule(typeof(Second))]");

        result.Diagnostics.Should().BeEmpty();

        var catalog = Catalog(result.Output);

        catalog.Should().Contain("global::First()").And.Contain("global::Second()");
    }

    [Fact]
    public void RerunningWithoutChangesReusesCachedResults()
    {
        var compilation = Compile("services.AddConfixOptions<Mail>(configuration);", "");
        var options = new GeneratorDriverOptions(
            IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegistrationGenerator().AsSourceGenerator()],
            driverOptions: options);

        driver = driver.RunGenerators(compilation);
        driver = driver.RunGenerators(compilation.Clone());

        var outputs = driver.GetRunResult().Results.Single()
            .TrackedOutputSteps.SelectMany(step => step.Value)
            .SelectMany(step => step.Outputs);

        outputs.Should().NotBeEmpty()
            .And.OnlyContain(output => output.Reason == IncrementalStepRunReason.Cached);
    }

    private static (Compilation Output, ImmutableArray<Diagnostic> Diagnostics) Run(
        string registration,
        string assemblyAttribute = "")
    {
        var compilation = Compile(registration, assemblyAttribute);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new RegistrationGenerator().AsSourceGenerator());

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        return (output, diagnostics);
    }

    private static CSharpCompilation Compile(string registration, string assemblyAttribute)
    {
        var source = $$"""
            using Confix;
            using Microsoft.Extensions.Configuration;
            using Microsoft.Extensions.DependencyInjection;
            {{assemblyAttribute}}
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();
            {{registration}}
            [ConfixSection("Mail")]
            public class Mail { public string Host { get; set; } = ""; }
            """;

        var platform = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var paths = platform.Split(Path.PathSeparator)
            .Concat(Directory.GetFiles(AppContext.BaseDirectory, "Microsoft.Extensions.*.dll"))
            .Concat([
                typeof(ConfixSectionAttribute).Assembly.Location,
                typeof(ConfixOptionsExtensions).Assembly.Location
            ])
            .Distinct();

        return CSharpCompilation.Create(
            "CatalogTest",
            [CSharpSyntaxTree.ParseText(source)],
            paths.Select(p => MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
