using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace Confix.Tool.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        JsonNode node = JsonNode.Parse("5");

        node.Should().NotBeNull();
    }
}