using System.Text.Json.Nodes;
using Confix.Tool.Middlewares;
using Confix.Utilities;
using Confix.Variables;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Confix.Tool.Tests;

public class VariableProviderFactoryTests
{
    private readonly Mock<IGitService> _gitServiceMock;
    private readonly IServiceProvider _serviceProvider;

    public VariableProviderFactoryTests()
    {
        _gitServiceMock = new Mock<IGitService>();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_ => _gitServiceMock.Object)
            .BuildServiceProvider();
    }

    [Fact]
    public void CreateProvider_KnownProviderType_CreatesProvider()
    {
        // Arrange
        string providerType = "test";
        var factory = new VariableProviderFactory(
            _serviceProvider,
            new Dictionary<string, Factory<IVariableProvider>>()
            {
                { providerType, (_, c) => new LocalVariableProvider(c) },
            });

        VariableProviderConfiguration configuration = new()
        {
            Name = "irrelevant",
            Type = providerType,
            Configuration = JsonNode.Parse("""
                {
                    "path": "/path/to/file.json"
                }
                """)!
        };
        // Act
        var provider = factory.CreateProvider(configuration);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<LocalVariableProvider>();
    }

    [Fact]
    public void CreateProvider_UnknownProviderType_ExitException()
    {
        // Arrange
        var factory = new VariableProviderFactory(
            _serviceProvider,
            new Dictionary<string, Factory<IVariableProvider>>()
            {
                { "NOT-the-one-we-are-looking-for", (_, c) => new LocalVariableProvider(c) },
            });

        VariableProviderConfiguration configuration = new()
        {
            Name = "irrelevant",
            Type = "the-one-we-are-looking-for",
            Configuration = JsonNode.Parse("""
                {
                    "path": "/path/to/file.json"
                }
                """)!
        };

        // Act & Assert
        Assert.Throws<ExitException>(() => factory.CreateProvider(configuration));
    }
}
