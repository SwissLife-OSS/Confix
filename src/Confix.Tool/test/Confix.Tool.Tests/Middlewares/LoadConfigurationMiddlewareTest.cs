using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Moq;
using Snapshooter.Xunit;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class LoadConfigurationMiddlewareTest
{
    private readonly string _testRoot;
    private readonly string _testHome;
    private readonly string _monoRepo;
    private readonly string _testRepo;
    private readonly string _testProject;
    private readonly string _testComponent;
    private readonly string _confixRoot;
    private readonly string _testComponentConfig;
    private readonly string _testProjectConfig;
    private readonly string _testRepoConfig;
    private readonly string _testHomeConfig;

    public LoadConfigurationMiddlewareTest()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _testHome = Path.Combine(_testRoot, "home");
        _monoRepo = Path.Combine(_testRoot, "mono-repo");
        _testRepo = Path.Combine(_monoRepo, "repo");
        _testProject = Path.Combine(_testRepo, "project");
        _testComponent = Path.Combine(_testProject, "component");
        _confixRoot = Path.Combine(_testRoot, FileNames.ConfixRc);
        _testComponentConfig = Path.Combine(_testComponent, FileNames.ConfixComponent);
        _testProjectConfig = Path.Combine(_testProject, FileNames.ConfixProject);
        _testRepoConfig = Path.Combine(_testRepo, FileNames.ConfixSolution);
        _testHomeConfig = Path.Combine(_testHome, FileNames.ConfixRc);

        Directory.CreateDirectory(_testHome);
        Directory.CreateDirectory(_testRepo);
        Directory.CreateDirectory(_testProject);
        Directory.CreateDirectory(_testComponent);
    }

    [Fact]
    public async Task Should_Discover_Component()
    {
        // arrange
        await SetupHome();
        await SetupConfixRoot();
        await SetupRepo();
        await SetupProject();
        await SetupComponent();
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        var featureCollection = new FeatureCollection();
        var executionContext = new StubExecutionContext(_testComponent, _testHome);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);
        middelwareContext.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        middelwareContext.SetupGet(x => x.Status).Returns(Mock.Of<IStatus>());
        middelwareContext.SetupGet(x => x.Execution).Returns(executionContext);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var middleware = new LoadConfigurationMiddleware();
        var isInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        Assert.True(isInvoked);
        var feature = featureCollection.Get<ConfigurationFeature>();
        Assert.NotNull(feature);
        Assert.Equal(ConfigurationScope.Component, feature.Scope);
        Assert.NotNull(feature.Component);
        Assert.NotNull(feature.Project);
        Assert.NotNull(feature.Solution);
        new
            {
                feature.Component,
                feature.Project,
                feature.Solution
            }.ToJsonString()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_Discover_Project()
    {
        // arrange
        await SetupHome();
        await SetupConfixRoot();
        await SetupRepo();
        await SetupProject();
        await SetupComponent();
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        var featureCollection = new FeatureCollection();
        var executionContext = new StubExecutionContext(_testProject, _testHome);
        middelwareContext.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);
        middelwareContext.SetupGet(x => x.Status).Returns(Mock.Of<IStatus>());
        middelwareContext.SetupGet(x => x.Execution).Returns(executionContext);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var middleware = new LoadConfigurationMiddleware();
        var isInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        Assert.True(isInvoked);
        var feature = featureCollection.Get<ConfigurationFeature>();
        Assert.NotNull(feature);
        Assert.Equal(ConfigurationScope.Project, feature.Scope);
        Assert.Null(feature.Component);
        Assert.NotNull(feature.Project);
        Assert.NotNull(feature.Solution);
        new
            {
                feature.Component,
                feature.Project,
                feature.Solution
            }.ToJsonString()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_Discover_MonoRepo()
    {
        // arrange
        await SetupHome();
        await SetupConfixRoot();
        await SetupMonoRepo();
        await SetupProject();
        await SetupComponent();
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        var featureCollection = new FeatureCollection();
        var executionContext = new StubExecutionContext(_testProject, _testHome);
        middelwareContext.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);
        middelwareContext.SetupGet(x => x.Status).Returns(Mock.Of<IStatus>());
        middelwareContext.SetupGet(x => x.Execution).Returns(executionContext);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var middleware = new LoadConfigurationMiddleware();
        var isInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        Assert.True(isInvoked);
        var feature = featureCollection.Get<ConfigurationFeature>();
        Assert.NotNull(feature);
        Assert.Equal(ConfigurationScope.Project, feature.Scope);
        Assert.Null(feature.Component);
        Assert.NotNull(feature.Project);
        Assert.NotNull(feature.Solution);
        new
            {
                feature.Component,
                feature.Project,
                feature.Solution
            }.ToJsonString()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_Discover_Solution()
    {
        // arrange
        await SetupHome();
        await SetupConfixRoot();
        await SetupRepo();
        await SetupProject();
        await SetupComponent();
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        var featureCollection = new FeatureCollection();
        var executionContext = new StubExecutionContext(_testRepo, _testHome);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);
        middelwareContext.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        middelwareContext.SetupGet(x => x.Status).Returns(Mock.Of<IStatus>());
        middelwareContext.SetupGet(x => x.Execution).Returns(executionContext);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var middleware = new LoadConfigurationMiddleware();
        var isInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        Assert.True(isInvoked);
        var feature = featureCollection.Get<ConfigurationFeature>();
        Assert.NotNull(feature);
        Assert.Equal(ConfigurationScope.Solution, feature.Scope);
        Assert.Null(feature.Component);
        Assert.NotNull(feature.Project);
        Assert.NotNull(feature.Solution);
        new
            {
                feature.Component,
                feature.Project,
                feature.Solution
            }.ToJsonString()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_Discover_None()
    {
        // arrange
        await SetupHome();
        await SetupConfixRoot();
        await SetupRepo();
        await SetupProject();
        await SetupComponent();
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        var featureCollection = new FeatureCollection();
        var executionContext = new StubExecutionContext(_monoRepo, _testHome);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);
        middelwareContext.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        middelwareContext.SetupGet(x => x.Status).Returns(Mock.Of<IStatus>());
        middelwareContext.SetupGet(x => x.Execution).Returns(executionContext);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var middleware = new LoadConfigurationMiddleware();
        var isInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        Assert.True(isInvoked);
        var feature = featureCollection.Get<ConfigurationFeature>();
        Assert.NotNull(feature);
        Assert.Equal(ConfigurationScope.None, feature.Scope);
        Assert.Null(feature.Component);
        Assert.Null(feature.Solution);
        Assert.NotNull(feature.Project);
        new
            {
                feature.Component,
                feature.Project,
                feature.Solution
            }.ToJsonString()
            .MatchSnapshot();
    }

    private async Task SetupComponent()
    {
        await File.WriteAllTextAsync(_testComponentConfig,
            """
                {
                    "name": "TestComponent",
                    "inputs": [
                        {
                            "type": "graphql",
                            "additional": "property"
                        },
                        {
                            "type": "dotnet",
                            "additional2": "property"
                        }
                    ],
                    "outputs": [{
                        "type": "schema",
                        "additional2": "property"
                    }]
                }
            """);
    }

    private async Task SetupProject()
    {
        await File.WriteAllTextAsync(_testProjectConfig,
            """
                {
                    "environments": [
                        "dev", "uat", "prod"
                    ]
                }
            """);
    }

    private async Task SetupRepo()
    {
        await File.WriteAllTextAsync(_testRepoConfig,
            """
                {
                    "project": {
                        "environments": [
                            "dev", "prod", { "name": "uat"}
                        ]
                    }
                }
            """);
    }

    private async Task SetupMonoRepo()
    {
        await File.WriteAllTextAsync(Path.Combine(_monoRepo, FileNames.ConfixSolution),
            """
                {
                    "project": {
                        "environments": [
                            "dev", "prod", { "name": "uat"}
                        ]
                    }
                }
            """);
    }

    private async Task SetupConfixRoot()
    {
        await File.WriteAllTextAsync(_confixRoot,
            """
                {
                    "isRoot":false,
                    "project": {
                        "environments": [
                            "dev", { "name": "prod"}
                        ]
                    }
                }
            """);
    }

    private async Task SetupHome()
    {
        await File.WriteAllTextAsync(_testHomeConfig,
            """
                {
                    "project": {
                        "environments": [
                            { "name": "dev"}
                        ]
                    }
                }
            """);
    }
}
