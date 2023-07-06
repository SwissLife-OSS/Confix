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
    MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA9Jz6R/tD4FeK4EF3C6QN
    5NA08O6ju8Eqi48SUogpO6bN9VfIVP66mBcD/3B25A1ZCPkXPzqXqlsr0HDd4RYz
    mDNFoVfwzcYvn3F2zdEe3SCXGqhUlJGKCYh2vI1ds9vd/0FlR2PDsvGLctFHJt/v
    eT2hC5Ossk+i8P2W7fJ9e4WmZQMUeLSCOYVdJzenssA3MztJBHrJa9G7ENMwtcrE
    fTWk1SB2ld/BWBboBuJJj953yYy4bufTssmepnmna1eaW0E33OkpB3Wra1kN55/G
    R2h+v4UQol1pvluT87tGek9inwdtObE3/97wAtsWsQtvo0fd0MYGOm3OnMsNfp5u
    OQIDAQAB
    -----END PUBLIC KEY-----
    """;

    private const string PRIVATE_KEY_PW = """
    -----BEGIN ENCRYPTED PRIVATE KEY-----
    MIIFHzBJBgkqhkiG9w0BBQ0wPDAbBgkqhkiG9w0BBQwwDgQIR1OOIDfHPmoCAggA
    MB0GCWCGSAFlAwQBKgQQsuPEEC++b0XSlNOBq/HeugSCBNALxFgYpxFoHTuRbShG
    Lk5BIfyryU81wxS05Md0GmiHJezN0SLSjGyW1P2UcP/QQlM/9uskAdLpn6rJLlHA
    eQhY6Pw8hYyDeVlUMuwWObLuG72sSYj4wSjbRdp6fVYY5c4js50bpAbN3bA78ANs
    9qhDvJ8W/Dx1uU7O1H1mOzyvOadLQM/bfD6rj0iqOqVKDGtb5C0ktHk2T1HQF2qU
    nF5O6D/7SvFk2WFzqP4m0jojgVLmpRFYBwU2N6xNqRHFq0JVVA9uQAA54hh7c+Lm
    3bepsV82b+NlOvS3ZQPibA+F80OxiMs5kWDdjdXXZIpwRlmNtj3/o2GabcSTOIYI
    s+NGsh1cFRe737uUBcm1lgOLoHGn03ivwHUKoP0SIw7NMa86c6LAsRlXfne3+Gu8
    9drHczccN24S84xGY5zarGPwLUmtakp8UevcAgfvPcvoQpTW/QDrSlyER9dIeeMF
    6VV27/nbDJjBaTyK4aGnSsZGUMPczFZRuahU9aHaxl4942Q6UqOlwuvkJ5KlUmJx
    dXI+PjZi64Ytcxu/BEoJj7zZOOMdtUhxBJGoKZ4BJkNuQU2cezDfByuyxBkEqgAb
    hSK7r1jncKFxICimHxvfJEq+fuoY9GSk/kAhzsxq9X6zBeAFZW++UGiAa8/YsEGN
    izj8Sb+N/cGswPMnTjuBkMQcXr/6aoX/iBEEc2AtjX6vICy0Wb/EayZEHiFEC2Je
    6l+d5OpbdUqwwyQzPCstyQO6LdgX/Sum8+RYvlYYE45nN9YnFq7aaD2KQ5Gt/xAo
    NrfxGoDjAlT8buqA7KjvQW5oGrSHWJm4hlXgamcfrun401H8zgG4d23bMvjQxVVW
    8oxqCTcDxyXmwQ0RhJh58IYUbYJK1abM5U+knjvB98i6E9ubDMe3rBmZNee2fnRU
    po3fpdlj8th9bhk5qA+3BrxBZpoKxpGXzDILfSbTfAtgJnK0i0UUH9aYWBM452nA
    MHCLEagTLMS+7CE3aK1qLTlo/Y4w5u+/ju349q1xg0QDd2vRURalPsxbFbgPPfqI
    4KcIroe85V6YDhFAFdsU0iDD3do8r2EhHsYnLl6UmsjX5tjq2aE+z49C1CZUrb0S
    xvNPkjOpRDZJJkpJmS9V9tNFc+zCsutbUTYOJDn3RcmsUo0JNlaPPyqHoVaDKtMW
    N81Eor+pshgjaRXXT1DBuyy9gAaNWN31BM8ZKGnapMFwGmZArFOXSdUrA8IuiabZ
    XcJDn2bY+wmEgp9Xdfv/ArJIp5G6eZwsMfWxi2aZJFOdxFmrOf3bdTzq8DIHnwXk
    VNEsK95IVC0Nz/Yw+xreT95VGospgDzrgv1RcAdQinIgFbCVJ9yq8hAsRSACio1A
    5x4RzutaIacmKQaXvPBPMOa6JOBj6lF4dfnMFB1Gi2eO3kWsVFIG0068T2R82qkM
    JU6Itva5k/KqDdTezDGJm0FdLwOEY84LTebjpyICgNPGaEq8OJlv3dEnDch6R0Vk
    9pPNCsX+6ux4Cc4JMD1+oQkqLsGRUSZ8N+NvodIj+xIVVf7z0JsVR706Hj2MtD89
    OOIwT11HGubok6qB1t/iYamUtkeP1kIify6dllrpF5L7HQZRW0CJkd5XtWYuyN7c
    K08/AL1y9BdGR8Oyt+MAOgYhDQ==
    -----END ENCRYPTED PRIVATE KEY-----
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
    public async Task ResolveAsync__WrongPassword_Should_Exit()
    {
        // arrange
        SecretVariableProvider provider = new(new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.Pkcs1,
            PUBLIC_KEY_PW,
            null,
            PRIVATE_KEY_PW,
            null,
            "wrongPassword")
        );

        var initialSecret = JsonValue.Create("I'm super secret string")!;
        var encryptedSecret = await provider.SetAsync("not relevant here", initialSecret, default);

        // act

        var ex = await Assert.ThrowsAsync<ExitException>(async ()
            => await provider.ResolveAsync(encryptedSecret, default));

        // assert
        ex.Message.Should().Be("Invalid password for private key");
    }
}
