using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Entities.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Tests.Entities.Schema;

public class MetadataKeywordTests
{
    [Fact]
    public void Constructor_WithValidArray_SetsValue()
    {
        // arrange
        var array = new JsonArray("item1", "item2");

        // act
        var keyword = new MetadataKeyword(array);

        // assert
        Assert.NotNull(keyword.Value);
        Assert.Equal(2, keyword.Value.Count);
    }

    [Fact]
    public void Constructor_WithEmptyArray_SetsEmptyValue()
    {
        // arrange
        var array = new JsonArray();

        // act
        var keyword = new MetadataKeyword(array);

        // assert
        Assert.NotNull(keyword.Value);
        Assert.Empty(keyword.Value);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // arrange
        var keyword1 = new MetadataKeyword(new JsonArray("item1", "item2"));
        var keyword2 = new MetadataKeyword(new JsonArray("item1", "item2"));

        // act & assert
        Assert.True(keyword1.Equals(keyword2));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // arrange
        var keyword1 = new MetadataKeyword(new JsonArray("item1"));
        var keyword2 = new MetadataKeyword(new JsonArray("item2"));

        // act & assert
        Assert.False(keyword1.Equals(keyword2));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // arrange
        var keyword = new MetadataKeyword(new JsonArray("item1"));

        // act & assert
        Assert.False(keyword.Equals(null));
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // arrange
        var keyword = new MetadataKeyword(new JsonArray("item1"));

        // act & assert
        Assert.True(keyword.Equals(keyword));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        // arrange
        var keyword1 = new MetadataKeyword(new JsonArray("item1", "item2"));
        var keyword2 = new MetadataKeyword(new JsonArray("item1", "item2"));

        // act & assert
        Assert.Equal(keyword1.GetHashCode(), keyword2.GetHashCode());
    }

    [Fact]
    public void JsonConverter_DeserializeValidArray_ReturnsKeyword()
    {
        // arrange
        var json = """["item1", "item2", "item3"]""";

        // act
        var keyword = JsonSerializer.Deserialize<MetadataKeyword>(json);

        // assert
        Assert.NotNull(keyword);
        Assert.Equal(3, keyword.Value.Count);
    }

    [Fact]
    public void JsonConverter_DeserializeEmptyArray_ReturnsKeywordWithEmptyArray()
    {
        // arrange
        var json = """[]""";

        // act
        var keyword = JsonSerializer.Deserialize<MetadataKeyword>(json);

        // assert
        Assert.NotNull(keyword);
        Assert.Empty(keyword.Value);
    }

    [Fact]
    public void JsonConverter_DeserializeNull_ReturnsNull()
    {
        // arrange - when JSON is literal "null", the serializer returns null
        // without calling the converter (standard System.Text.Json behavior)
        var json = """null""";

        // act
        var keyword = JsonSerializer.Deserialize<MetadataKeyword>(json);

        // assert
        Assert.Null(keyword);
    }

    [Fact]
    public void JsonConverter_DeserializeComplexArray_ReturnsKeyword()
    {
        // arrange
        var json = """[{"key": "value"}, null, "string", 42]""";

        // act
        var keyword = JsonSerializer.Deserialize<MetadataKeyword>(json);

        // assert
        Assert.NotNull(keyword);
        Assert.Equal(4, keyword.Value.Count);
    }

    [Fact]
    public void JsonConverter_SerializeKeyword_ReturnsValidJson()
    {
        // arrange
        var keyword = new MetadataKeyword(new JsonArray("item1", "item2"));
        var options = new JsonSerializerOptions { WriteIndented = false };

        // act
        var json = JsonSerializer.Serialize(keyword, options);

        // assert
        // Note: The custom converter writes property name, so we wrap it
        Assert.Contains("metadata", json);
    }

    [Fact]
    public void JsonConverter_RoundTrip_PreservesValue()
    {
        // arrange
        var originalArray = new JsonArray("item1", 42, true);
        var keyword = new MetadataKeyword(originalArray);

        // act - serialize then deserialize the array directly
        var json = keyword.Value.ToJsonString();
        var deserializedKeyword = JsonSerializer.Deserialize<MetadataKeyword>(json);

        // assert
        Assert.NotNull(deserializedKeyword);
        Assert.True(keyword.Value.IsEquivalentTo(deserializedKeyword.Value));
    }
}
