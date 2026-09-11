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
        result.Output.SyntaxTrees.Last().ToString().Should().Contain("new global::Setup().Configure(services, configuration);");
    }

    [Fact]
    public void RerunningWithoutChangesReusesCachedResults()
    {
        var compilation = Compile("services.AddConfixOptions<Mail>(configuration);", "");
        var options = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegistrationGenerator().AsSourceGenerator()], driverOptions: options);

        driver = driver.RunGenerators(compilation);
        driver = driver.RunGenerators(compilation.Clone());

        var outputs = driver.GetRunResult().Results.Single()
            .TrackedOutputSteps.SelectMany(step => step.Value)
            .SelectMany(step => step.Outputs);
        outputs.Should().NotBeEmpty()
            .And.OnlyContain(output => output.Reason == IncrementalStepRunReason.Cached);
    }

    private static (Compilation Output, System.Collections.Immutable.ImmutableArray<Diagnostic> Diagnostics) Run(
        string registration, string assemblyAttribute = "")
    {
        var compilation = Compile(registration, assemblyAttribute);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new RegistrationGenerator().AsSourceGenerator());
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
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Concat(Directory.GetFiles(AppContext.BaseDirectory, "Microsoft.Extensions.*.dll"))
            .Concat([typeof(ConfixSectionAttribute).Assembly.Location, typeof(ConfixOptionsExtensions).Assembly.Location])
            .Distinct();
        return CSharpCompilation.Create("CatalogTest", [CSharpSyntaxTree.ParseText(source)],
            paths.Select(p => MetadataReference.CreateFromFile(p)), new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
