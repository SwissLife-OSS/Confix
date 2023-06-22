using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Json.More;

namespace Confix.Tool.Tests;

public class SecretVariableProviderTests
{
    private const string PUBLIC_KEY = """
    -----BEGIN PUBLIC KEY-----
    MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgG7OACVD8XNhVBdDToLsUp6NaxBY
    ERaXIjfn9Tawf+Q2zQVJNC2w0b4/ZUP6BkIhjzjG+a98lFwMhijMPoUBTS7XHLGp
    3lEKmcpJQNb5dkpnJc6/Aetxba4OJKxUK0DThe/6mY0y8prO4QRZFEektN2mLBdp
    hRE7gfA8fH9EDN3HAgMBAAE=
    -----END PUBLIC KEY-----
    """;

    private const string PRIVATE_KEY = """
    -----BEGIN RSA PRIVATE KEY-----
    MIICXAIBAAKBgG7OACVD8XNhVBdDToLsUp6NaxBYERaXIjfn9Tawf+Q2zQVJNC2w
    0b4/ZUP6BkIhjzjG+a98lFwMhijMPoUBTS7XHLGp3lEKmcpJQNb5dkpnJc6/Aetx
    ba4OJKxUK0DThe/6mY0y8prO4QRZFEektN2mLBdphRE7gfA8fH9EDN3HAgMBAAEC
    gYAyqSUP5LykUD+uUyu2WG5955Kn6lwFxBv1C6zl0FTo9tVOMWYV7d436axXJB1w
    Zv+gqfjG72K1o1RDmv6KuUmcIIPcAv0rXY54MqdryRiRvvbi8b/ojy7Ls8ExYaIY
    XzROKqetP/kYTNf4QQvvEhrtiqaB5sRtCqybWS1t6rm7AQJBAMizh5WZa8NcBQ0f
    I0kxqrqpcqcyCMeg5A2e9ScSkNNRX4C8YbVJJa0I/XSrT5hw4brv1H3yXCY4O40y
    9fysmmECQQCNVZy1stk5wsvSKQdPCf2N/zJt1rNW7sSUqROGd4ERfeQKitg+wtjE
    nStY+iP6i9JQstd3Fke7onB1HaoEOfknAkEAjXfWg0GQbzUGrngbVDV5JXfZRDcF
    b0leVqeMIA17HikGi2S97p3vu6dRmJJEWax/wFfazSgvghUzDNU2BPPZIQJAHMOh
    wzEFGMZWaQ1EmYd0/SNFBim+EiFCDOdkO+eycvbmJGchn5RUPZ+nJNKz49f1E8ty
    IqB8NOnYbV1+LqIF3QJBAK49P1l7vFf5oLsmbDUDm02lCX34+OFHG73v8KovSgHP
    N1k9i5Rx/RBiELyShk7r+hSG/hoXE4qN7xTa0C4ct84=
    -----END RSA PRIVATE KEY-----
    """;

    [Fact]
    public async Task ListAsync_IsNonSense_ReturnsEmptyArrayAsync()
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            null,
            null,
            null,
            null)
        );

        // act
        var result = await provider.ListAsync(default);

        // assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("I'm super secret string")]
    [InlineData(42)]
    public async Task ResolveAsync_Should_Decrypt(object value)
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            PUBLIC_KEY,
            null,
            PRIVATE_KEY,
            null)
        );

        var initialSecret = JsonValue.Create(value)!;
        var encryptedSecret = await provider.SetAsync("not relevant here", initialSecret, default);

        // act
        var decrypted = await provider.ResolveAsync(encryptedSecret, default);

        // assert
        Assert.True(decrypted.IsEquivalentTo(initialSecret));
    }
}
