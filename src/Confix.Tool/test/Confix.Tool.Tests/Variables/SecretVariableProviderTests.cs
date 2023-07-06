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

    private const string PUBLIC_KEY_PW = """
    -----BEGIN PUBLIC KEY-----
    MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2hQAs2qhmutCYevj+qRV
    RTSwIv3bwgy/L/jQpc7zU4aJNTQRJ5IEgy09DblAvZePE+v6koIoztY9WfrYoST9
    jQQrJo2nXnXZ7sMVUQFf2jzpZcgDqn1alcIoB3RSAXI4/A8mspc7dIl1vMWu4j3C
    CHu5Oto2AF6FCjdsU3MDfcrbUKqWLyezN/xcju1uHHJ+WHpScJGAm0382+TN5cOO
    VlSUJJjCyIuYoANzbjDHt7+JCf/Oe20UDcd8DFgigbx9d5y2cYH2tXq95yxqjCbC
    DrBbZGk8zWfs/uJLI5ClW2RoCxR8pjjRpXKPkXDjBG+uTuJaH0bsV6hmFVn/lduQ
    iQIDAQAB
    -----END PUBLIC KEY-----
    """;

    private const string PRIVATE_KEY_PW = """
    -----BEGIN RSA PRIVATE KEY-----
    Proc-Type: 4,ENCRYPTED
    DEK-Info: AES-256-CBC,EA55E21E2135603C72BD60B4785B755E

    p/vETFy5r98RGdEpXGTTBFYouh36HeGT/AF7CgnsHLN8Ds7cxXCwkaNhFwoWwLEG
    mkVCCcgpIZhOh31ni1pypuZ51vWdxepUaO6dUvVOmSHmSgNjlU0i3S1YIB1UVr7Z
    bvq9fMlHI/5+rPZDaqg1NsFhx8pM23Dmygjki99NHvFZATZ4gFU/Y9BKywKHsh/b
    r5VVtQx2i8ikZ5nzU1X7Oqy/d66s9vHAH1fZb+r9XGVN0VLgKQ+zoQy//rAU2QE+
    vJSGpaVe61aSpLGowlOh5IAnDFXYvMFgSJSUMsUr6L/41p1/GNcIUwz0cwZaiulv
    Fc4pIxJcRRMQg0yLqvDu9fDLkJT7DHfANGPD6s99lD6pMCqeFmnyST2iWPSkGzSE
    N83wggWm2MF72200qFOquqvakIedZx6Knh1eUibpjYMZcrG9o0XfSH49LpsftgXt
    WSnEUOKKfwSNanR7Spozt1c60pCnQsjGnH9dIqUYdYNAGPES42h0VAZ0N/orCPjr
    73+loOAYOekqGqZ8oHTUbqCJWofQfw4j8z5RH2MkOO0vV/r8RLquWbJSs0T3r/Oy
    9KKGmZR4QK/4NB7Kfmb2JnZ6Aw5idftvCKxe7ej6fHW+7hGMQv8b/D80mkPTgyy1
    XlZk4qO4QZjAkE2m/F7w6PDc9OSI2Kbn8HZ18DE0XHOjRs02643525Lmpy9lJ1yW
    L4pnyz2Koejmi4LCxfIJzZp8rncOIewR5QAAsqvLzt1QxLpNxcJfeOdxF+TW7Wrr
    gWvEzVINL4A+VA5HighP6HehSweRZa9DBiSklcyUluzGpt6Np6IXHb3kpgN2rxGA
    uKALzUlS81ichAYW1Qpo/S6yqMOJfExBOM8gv39nzw0y7ahrBu9srMu2DHajzG1B
    DJFib8+6qrqwzCnMnP2/uZNpIBa0PF42LTaZ//5QeBWxmzpatDM3e1XTqVKHpYae
    fHnKJ6mRrggfCWsSjegzYX3Dpsz6jlmRQFfG/PdvDDvrl35ktNTBHhntQ2z+M6np
    aBzWucYAsH/+7zkIykccUCTzvJckG3Qc4DZxBICwBfrfukZtsQGb27KBru4jxZbK
    RtXQ4vcguxQXUOw3ZY3O46DKyIrUWJimC4sxt9s46sxy5Wav0iJwt3EaJ9DGC+Hg
    u3fmUsCXHktvaKVdH/4iBz/vOLcejtz4XxWBB3RqMjZGgk1fRuQWG36vnZfoMERL
    xF+fg4Op3MbNWT6y42LHsIku4kQ6xyBRNX5VWbHI1RULxJpHnyOYTrwPnORlSmoJ
    F22634/DlEZHMIgXEbf9+Try617G/Wi93u/IVvfo6bixj2q8FrUnHvVyuQ3TeKcY
    0LVs4eZk1QtCRFArnqRUhjkG2GXlmKyxrbqZrZ4TO//VOI2Ci8916XBzLKazUmfc
    mBUHUEDCONOurmjSFnQIHWyC40hju1SaVKxM84SN1WbQ7tHaBoPkIgvY3TsP9htM
    aAFpYrZEE9ACtKZp0xSUmPnRyEsBeGgRgiD+Pm7P598HOTXFpvSqvLXm/GDl/PgQ
    nSXDXuOQjLBIBMrAJIXl06ZdmCtOIg166IJ6cY+d/swPIAu71MaQW/SC+xW1ZhwZ
    -----END RSA PRIVATE KEY-----
    """;

    private const string PASSWORD = "CONFIX";

    [Fact]
    public async Task ListAsync_IsNonSense_ReturnsEmptyArrayAsync()
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderDefinition(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            null,
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
        SecretVariableProvider provider = new(new SecretVariableProviderDefinition(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            PUBLIC_KEY,
            null,
            PRIVATE_KEY,
            null,
            null)
        );

        var initialSecret = JsonValue.Create(value)!;
        var encryptedSecret = await provider.SetAsync("not relevant here", initialSecret, default);

        // act
        var decrypted = await provider.ResolveAsync(encryptedSecret, default);

        // assert
        Assert.True(decrypted.IsEquivalentTo(initialSecret));
    }

    [Theory]
    [InlineData("I'm super secret string")]
    [InlineData(42)]
    public async Task ResolveAsync_Should_DecryptWithPassword(object value)
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.Pkcs1,
            PUBLIC_KEY_PW,
            null,
            PRIVATE_KEY_PW,
            null,
            PASSWORD)
        );

        var initialSecret = JsonValue.Create(value)!;
        var encryptedSecret = await provider.SetAsync("not relevant here", initialSecret, default);

        // act
        var decrypted = await provider.ResolveAsync(encryptedSecret, default);

        // assert
        Assert.True(decrypted.IsEquivalentTo(initialSecret));
    }

    [Fact]
    public async Task ResolveAsync_Should_DecryptWrong()
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.Pkcs1,
            PUBLIC_KEY_PW,
            null,
            PRIVATE_KEY_PW,
            null,
            "wrong")
        );

        var initialSecret = JsonValue.Create("I'm super secret string")!;
        var encryptedSecret = await provider.SetAsync("not relevant here", initialSecret, default);

        // act

        var ex = await Assert.ThrowsAsync<ExitException>(async ()
            => await provider.ResolveAsync(encryptedSecret, default));

        // assert
        ex.Message.Should().Be("Could not decrypt private key. Please check your password.");
    }
}
