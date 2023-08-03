using Confix.Inputs;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;
using Confix.Tool.Schema;
using Snapshooter.Xunit;
using static Confix.Tool.Middlewares.ConfigurationScope;
using Comp = Confix.Tool.Abstractions.Component;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class ValidationMiddlewareTests : IDisposable
{
    private ProjectDefinition _project;
    private readonly SolutionDefinition _solution;
    private readonly ISchemaStore _schemaStore;
    private readonly MiddlewareDelegate _next;
    private bool _nextWasCalled;
    private TestMiddlewareContext _context;

    public ValidationMiddlewareTests()
    {
        _schemaStore = new SchemaStore();
        _context = new TestMiddlewareContext();
        _project = TestHelpers.CreateProjectDefinition(
            directory: _context.Directories.Content.Append("solution/project"));

        _solution = TestHelpers.CreateSolutionDefinition(
            directory: _context.Directories.Content.Append("solution"));

        _nextWasCalled = false;
        _next = c =>
        {
            _nextWasCalled = true;
            return Task.CompletedTask;
        };
    }

    [Fact]
    public async Task Should_ReturnEarly_WhenNot_Project()
    {
        // arrange
        var configuration = TestHelpers.CreateConfigurationFeature(
            scope: ConfigurationScope.Project,
            solution: _solution);
        var files = new ConfigurationFileFeature();
        _context.Features.Set(configuration);
        _context.Features.Set(files);

        var middleware = CreateMiddleware();

        // act
        var ex = await Record.ExceptionAsync(() => middleware.InvokeAsync(_context, _next));

        // 
        var exitException = Assert.IsType<ExitException>(ex);
        Assert.Equal("No project was found", exitException.Message);
    }

    [Fact]
    public async Task Should_ReturnEarly_WhenNot_Solution()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            scope: ConfigurationScope.Project,
            project: _project);
        var files = new ConfigurationFileFeature();
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        // act
        var ex = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, _next));

        // 
        var exitException = Assert.IsType<ExitException>(ex);
        Assert.Equal("No solution was found", exitException.Message);
    }

    [Fact]
    public async Task Should_ReturnEarly_WhenNotInProjectScope()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers
            .CreateConfigurationFeature(scope: Solution, solution: _solution, project: _project);
        var files = new ConfigurationFileFeature();
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        // act
        var ex = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, _next));

        // 
        var exitException = Assert.IsType<ExitException>(ex);
        Assert.Equal("Scope has to be a project", exitException.Message);
    }

    [Fact]
    public async Task Should_ReturnEarly_When_NoSchemaFound()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);
        var files = new ConfigurationFileFeature();
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        // act
        var ex = await Record.ExceptionAsync(() => middleware.InvokeAsync(context, _next));

        // 
        var exitException = Assert.IsType<ExitException>(ex);
        Assert.Equal(
            "The schema for the project '__Default' could not be found. Call 'confix reload' to generate the schema.",
            exitException.Message);
    }

    [Fact]
    public async Task Should_CompleteAndCallNext_When_NoConfigurationFiles()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);
        var files = new ConfigurationFileFeature();
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        await SetupSchema(""" 
        type Configuration { schema: String }
        """);

        // act
        await middleware.InvokeAsync(context, _next);

        // 
        Assert.True(_nextWasCalled);
    }

    [Fact]
    public async Task Should_Validate_CorrectSchema()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);

        var file = context.Directories.Content.AppendFile("solution/project/configuration.json");
        var files = new ConfigurationFileFeature()
        {
            Files = { new ConfigurationFile() { InputFile = file, OutputFile = file } }
        };
        file.Directory!.EnsureFolder();
        await File.WriteAllTextAsync(file.FullName, """ { "schema": "bar"}  """);
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        await SetupSchema(""" 
        type Configuration { schema: String }
        """);

        // act
        await middleware.InvokeAsync(context, _next);

        // 
        Assert.True(_nextWasCalled);
    }

    [Fact]
    public async Task Should_Validate_InvalidSchema()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);

        var file = context.Directories.Content.AppendFile("solution/project/configuration.json");
        var files = new ConfigurationFileFeature()
        {
            Files = { new ConfigurationFile() { InputFile = file, OutputFile = file } }
        };
        file.Directory!.EnsureFolder();
        await File.WriteAllTextAsync(file.FullName, """ { "schema": "bar"}  """);
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        await SetupSchema(""" 
        type Configuration { schema: Int! }
        """);

        // act
        var ex = await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(context, _next));

        // 
        Assert.Equal("The configuration files are invalid.", ex.Message);
        SnapshotBuilder.New().AddOutput(context).MatchSnapshot();
    }
    
    [Fact]
    public async Task Should_Validate_InvalidSchema_PartiallyIncorrect()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);

        var file = context.Directories.Content.AppendFile("solution/project/configuration.json");
        var files = new ConfigurationFileFeature()
        {
            Files = { new ConfigurationFile() { InputFile = file, OutputFile = file } }
        };
        file.Directory!.EnsureFolder();
        await File.WriteAllTextAsync(file.FullName, """ { "nullable": "bar"}  """);
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        await SetupSchema(""" 
        type Configuration { schema: Int!, nullable: String }
        """);

        // act
        var ex = await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(context, _next));

        // 
        Assert.Equal("The configuration files are invalid.", ex.Message);
        SnapshotBuilder.New().AddOutput(context).MatchSnapshot();
    }
    
    [Fact]
    public async Task Should_Validate_InvalidSchema_DeepIncorrect()
    {
        // arrange
        var context = new TestMiddlewareContext();
        var configuration = TestHelpers.CreateConfigurationFeature(
            ConfigurationScope.Project,
            solution: _solution,
            project: _project);

        var file = context.Directories.Content.AppendFile("solution/project/configuration.json");
        var files = new ConfigurationFileFeature()
        {
            Files = { new ConfigurationFile() { InputFile = file, OutputFile = file } }
        };
        file.Directory!.EnsureFolder();
        await File.WriteAllTextAsync(file.FullName, """ { "deep": { "deeper" : {"fail":1}}}  """);
        context.Features.Set(configuration);
        context.Features.Set(files);

        var middleware = CreateMiddleware();

        await SetupSchema(""" 
        type Configuration {
            deep: Deep
        }
        type Deep {
            deeper: Deeper
        }
        type Deeper {
            fail: String
            requiredButMissing: Int!
            nonRequired: Int
        }
        """);

        // act
        var ex = await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(context, _next));

        // 
        Assert.Equal("The configuration files are invalid.", ex.Message);
        SnapshotBuilder.New().AddOutput(context).MatchSnapshot();
    }

    private async Task SetupSchema(string graphQL)
    {
        var jsonSchema = SchemaHelpers.BuildSchema(graphQL).ToJsonSchema();
        await _schemaStore.StoreAsync(_solution, _project, jsonSchema, CancellationToken.None);
    }

    private ValidationMiddleware CreateMiddleware()
    {
        return new ValidationMiddleware(_schemaStore);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Dispose();
    }
}
