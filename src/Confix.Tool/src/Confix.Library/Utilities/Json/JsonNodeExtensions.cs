using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Confix.Tool;
using Json.Schema;
using Spectre.Console;

namespace Confix.Utilities.Json;

public static partial class JsonNodeExtensions
{
    public static bool TryGetNonNullPropertyValue(
        this JsonObject obj,
        string propertyName,
        [NotNullWhen(true)] out JsonNode? value)
    {
        if (obj.TryGetPropertyValue(propertyName, out var node) && node is not null)
        {
            value = node;
            return true;
        }

        value = null;
        return false;
    }

    public static JsonNode? Merge(this JsonNode? source, JsonNode? node)
        => (source, node) switch
        {
            (null, null) => null,

            (not null, null) => source,

            (null, not null) => node,

            (JsonObject sourceObject, JsonObject nodeObject) => Merge(sourceObject, nodeObject),

            (JsonArray sourceArray, JsonArray nodeArray) => Merge(sourceArray, nodeArray),

            (_, JsonValue nodeValue) => nodeValue,
            _ => throw new InvalidOperationException($"""
                    Cannot merge nodes of different types:
                    Source: {source.GetValueKind()}
                    Node: {node.GetValueKind()}
                """)
        };

    private static JsonArray Merge(this JsonArray source, JsonArray node)
    {
        var array = new JsonNode?[source.Count + node.Count];

        for (var i = 0; i < source.Count; i++)
        {
            array[i] = source[i]?.Copy();
        }

        for (var i = 0; i < node.Count; i++)
        {
            array[i + source.Count] = node[i]?.Copy();
        }

        return new JsonArray(array);
    }

    private static JsonObject Merge(JsonObject source, JsonObject node)
    {
        var obj = new JsonObject();

        foreach (var (key, value) in source)
        {
            obj[key] = value.Copy();
        }

        foreach (var (key, value) in node)
        {
            if (obj.TryGetPropertyValue(key, out var existingValue))
            {
                obj[key] = existingValue.Merge(value.Copy());
            }
            else
            {
                obj[key] = value.Copy();
            }
        }

        return obj;
    }

    public static JsonNode SetValue(
        this JsonNode node,
        string path,
        JsonNode value)
    {
        var segments = new List<object>();
        foreach (var segment in path.Split('.'))
        {
            var match = ParseSegmentRegex().Match(segment);
            if (match.Success)
            {
                segments.Add(match.Groups["name"].Value);
                segments.Add(int.Parse(match.Groups["index"].Value));
            }
            else
            {
                segments.Add(segment);
            }
        }

        var current = node;
        var currentPath = "/";

        for (var i = 0; i < segments.Count - 1; i++)
        {
            var segment = segments[i];
            if (segment is string stringSegment)
            {
                if (current is not JsonObject)
                {
                    throw new ExitException(
                        $"Could not set value in file because the path {path.EscapeMarkup()} is not an object");
                }

                current[stringSegment] ??= GetNextNode();

                current = current[stringSegment];
                currentPath = Path.Join(currentPath, stringSegment);
            }
            else if (segment is int indexSegment)
            {
                if (current is not JsonArray arr)
                {
                    throw new ExitException(
                        $"Could not set value in file because the path {path.EscapeMarkup()} is not an array");
                }

                if (indexSegment >= arr.Count)
                {
                    var nextNode = GetNextNode();
                    arr.Add(nextNode);
                    current = nextNode;
                }
                else
                {
                    current = current[indexSegment];
                }

                currentPath = Path.Join(currentPath, indexSegment.ToString());
            }
            else
            {
                throw new InvalidOperationException($"Unknown segment type {segment.GetType()}");
            }

            continue;

            JsonNode GetNextNode()
            {
                if (segments.Count == i + 1)
                {
                    return value;
                }

                var nextSegment = segments[i + 1];
                if (nextSegment is string)
                {
                    return new JsonObject();
                }

                if (nextSegment is int)
                {
                    return new JsonArray();
                }

                return value;
            }
        }

        if (segments[^1] is string str && current is JsonObject)
        {
            current[str] = value;
        }
        else if (segments[^1] is int index && current is JsonArray)
        {
            current[index] = value;
        }
        else
        {
            throw new InvalidOperationException($"Unknown segment type {segments[^1].GetType()}");
        }

        return node;
    }

    public static async Task SerializeToStreamAsync(
        this JsonNode node,
        Stream stream,
        CancellationToken cancellationToken)
        => await JsonSerializer.SerializeAsync(
            stream,
            node,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            },
            cancellationToken);

    [GeneratedRegex(@"^(?<name>.+?)\[(?<index>\d+)]$")]
    private static partial Regex ParseSegmentRegex();

    /// <summary>
    /// Creates a deep copy of the JSON node.
    /// </summary>
    /// <param name="node">The JSON node to copy.</param>
    /// <returns>A deep copy of the JSON node.</returns>
    public static JsonNode? Copy(this JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return node.GetValueKind() switch
        {
            JsonValueKind.Object => CopyObject((JsonObject)node),
            JsonValueKind.Array => CopyArray((JsonArray)node),
            _ => JsonValue.Create(JsonSerializer.Deserialize<JsonElement>(node.ToJsonString()))
        };
    }

    private static JsonObject CopyObject(JsonObject source)
    {
        var copy = new JsonObject();
        foreach (var (key, value) in source)
        {
            copy[key] = Copy(value);
        }
        return copy;
    }

    private static JsonArray CopyArray(JsonArray source)
    {
        var copy = new JsonArray();
        foreach (var item in source)
        {
            copy.Add(Copy(item));
        }
        return copy;
    }

    /// <summary>
    /// Determines if two JSON nodes are equivalent.
    /// </summary>
    /// <param name="a">The first JSON node.</param>
    /// <param name="b">The second JSON node.</param>
    /// <returns>True if the nodes are equivalent; otherwise, false.</returns>
    public static bool IsEquivalentTo(this JsonNode? a, JsonNode? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        var aKind = a.GetValueKind();
        var bKind = b.GetValueKind();

        if (aKind != bKind)
        {
            return false;
        }

        return aKind switch
        {
            JsonValueKind.Object => AreObjectsEquivalent((JsonObject)a, (JsonObject)b),
            JsonValueKind.Array => AreArraysEquivalent((JsonArray)a, (JsonArray)b),
            JsonValueKind.String => AreValuesEquivalent(a, b),
            JsonValueKind.Number => AreValuesEquivalent(a, b),
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Null => true,
            _ => false
        };
    }

    private static bool AreObjectsEquivalent(JsonObject a, JsonObject b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (var (key, aValue) in a)
        {
            if (!b.TryGetPropertyValue(key, out var bValue))
            {
                return false;
            }

            if (!IsEquivalentTo(aValue, bValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreArraysEquivalent(JsonArray a, JsonArray b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (var i = 0; i < a.Count; i++)
        {
            if (!IsEquivalentTo(a[i], b[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreValuesEquivalent(JsonNode a, JsonNode b)
    {
        var aString = a.ToJsonString();
        var bString = b.ToJsonString();
        return aString == bString;
    }

    /// <summary>
    /// Gets a hash code for the JSON node that is consistent with equivalence comparison.
    /// </summary>
    /// <param name="node">The JSON node.</param>
    /// <returns>A hash code value.</returns>
    public static int GetEquivalenceHashCode(this JsonNode? node)
    {
        if (node is null)
        {
            return 0;
        }

        var kind = node.GetValueKind();

        return kind switch
        {
            JsonValueKind.Object => GetObjectHashCode((JsonObject)node),
            JsonValueKind.Array => GetArrayHashCode((JsonArray)node),
            JsonValueKind.String or JsonValueKind.Number => node.ToJsonString().GetHashCode(),
            JsonValueKind.True => true.GetHashCode(),
            JsonValueKind.False => false.GetHashCode(),
            JsonValueKind.Null => 0,
            _ => 0
        };
    }

    private static int GetObjectHashCode(JsonObject obj)
    {
        var hash = new HashCode();
        foreach (var (key, value) in obj.OrderBy(x => x.Key))
        {
            hash.Add(key);
            hash.Add(GetEquivalenceHashCode(value));
        }
        return hash.ToHashCode();
    }

    private static int GetArrayHashCode(JsonArray array)
    {
        var hash = new HashCode();
        foreach (var item in array)
        {
            hash.Add(GetEquivalenceHashCode(item));
        }
        return hash.ToHashCode();
    }
}
