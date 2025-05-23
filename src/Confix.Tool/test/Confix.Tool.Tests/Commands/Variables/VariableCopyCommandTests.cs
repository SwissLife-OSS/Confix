using System.CommandLine.Completions;
using System.Text.Json.Nodes;
using Confix.Inputs;
using Confix.Tool.Middlewares;
using Confix.Variables;

namespace Confix.Commands.Variables;

public class VariableCopyCommandTests
{
    private readonly IVariableProvider _first;
    private readonly IVariableProvider _second;
    private readonly TestConfixCommandline _cli;

    public VariableCopyCommandTests()
    {
        _first = new InMemoryVariableProvider();
        _second = new InMemoryVariableProvider();
        _cli = new TestConfixCommandline(x =>
        {
            x.AddVariableProvider("first", _ => _first);
            x.AddVariableProvider("second", _ => _second);
        });
    }

    [Fact]
    public async Task Should_Fail_When_VariableNameIsInvalid()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $firsta.b --to $second:a.b");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Fail_When_ToVariableNameIsInvalid()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $first:a.b --to $seconda.b");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Fail_When_FromProviderIsNotKnown()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $unknown:a.b --to $second:a.b");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Fail_When_ToProviderIsNotKnown()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $first:a.b --to $unknown:a.b");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Fail_When_VariableCouldNotBeResolved()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $first:a.b --to $second:a.b");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Copy_VariableFromOneProviderToTheOther()
    {
        // Arrange
        using var cli = _cli;
        const string beforeValue = "test";
        var node = (JsonNode?) beforeValue;
        await _first.SetAsync("a.b", node!, new VariableProviderContext(null, CancellationToken.None));

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("variable copy --from $first:a.b --to $second:a.b");

        // Assert
        var afterValue = await _second.ResolveAsync("a.b", new VariableProviderContext(null, CancellationToken.None));
        Assert.Equal(beforeValue, afterValue.ToString());
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Copy_VariableFromOneProviderToTheOther_When_DifferentEnvironment()
    {
        // Arrange
        using var cli = _cli;
        const string beforeValue = "test";
        var node = (JsonNode?) beforeValue;
        await _first.SetAsync("a.b", node!, new VariableProviderContext(null, CancellationToken.None));

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync(
            "variable copy --from $first:a.b --to $second:a.b --to-environment prod --environment dev");

        // Assert
        var afterValue = await _second.ResolveAsync("a.b", new VariableProviderContext(null, CancellationToken.None));
        Assert.Equal(beforeValue, afterValue.ToString());
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_Fail_When_Environment_DoesNotExists()
    {
        // Arrange
        using var cli = _cli;
        const string beforeValue = "test";
        var node = (JsonNode?) beforeValue;
        await _first.SetAsync("a.b", node!, new VariableProviderContext(null, CancellationToken.None));

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync(
            "variable copy --from $first:a.b --to $second:a.b --to-environment nope --environment dev");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    private const string _confixRc = """
        {
            "project": {
                 "environments": [
                   {
                     "name": "dev",
                     "enabled": true
                   },
                   {
                     "name": "prod"
                   }
                 ],
                 "variableProviders": [
                      {
                            "name": "first",
                            "type": "first"
                      },
                      {
                            "name": "second",
                            "type": "second"
                      }
                    ]
            }
        }
    """;
}

file class InMemoryVariableProvider : IVariableProvider
{
    private readonly Dictionary<string, JsonNode> _variables = new();

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return default;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListAsync(IVariableProviderContext context)
    {
        return Task.FromResult<IReadOnlyList<string>>(_variables.Keys.ToList());
    }

    /// <inheritdoc />
    public Task<JsonNode> ResolveAsync(string path, IVariableProviderContext context)
    {
        if (_variables.TryGetValue(path, out var value))
        {
            return Task.FromResult(value);
        }

        throw new VariableNotFoundException(path);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        IVariableProviderContext context)
    {
        return Task.FromResult<IReadOnlyDictionary<string, JsonNode>>(_variables
            .Where(x => paths.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value));
    }

    /// <inheritdoc />
    public Task<string> SetAsync(string path, JsonNode value, IVariableProviderContext context)
    {
        _variables[path] = value;
        return Task.FromResult(path);
    }
}
