using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace Confix.CodeFirst.Tests;

public sealed class RunnerBoundaryTests
{
    [Fact]
    public async Task CliUsesActualContractsWithoutStartingApplicationAndPreservesFailedOutput()
    {
        var root = FindRoot();
        var folder = Path.Combine(Path.GetTempPath(), "confix-boundary-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(folder, "Host.csproj"), $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="{root}/src/Confix.Options/Confix.Options.csproj" />
                    <ProjectReference Include="{root}/src/Confix.CodeGeneration/Confix.CodeGeneration.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
                    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.11" />
                    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.11" />
                  </ItemGroup>
                </Project>
                """);
            await File.WriteAllTextAsync(Path.Combine(folder, "Program.cs"), """
                using System.ComponentModel.DataAnnotations;
                using Confix;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;
                [assembly: ConfixModule(typeof(Setup))]
                File.WriteAllText("application-started", "incorrect");
                public sealed class Setup : IConfixModule
                {
                    public void Configure(IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddConfixOptions<Mail>(configuration).PostConfigure(o => o.Sender = "injected@example.com");
                    }
                }
                [ConfixSection("Mail")]
                public sealed class Mail
                {
                    [Required] public string Host { get; set; } = "";
                    [Required, EmailAddress] public string Sender { get; set; } = "";
                    #if DEBUG
                    [Range(1, 10)]
                    #else
                    [Range(20, 30)]
                    #endif
                    public int Port { get; set; } = 5;
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(folder, ".confixrc"), """
                {"isRoot":true,"project":{"validation":{"type":"dotnet-options"},"configurationFiles":[{"type":"appsettings","useUserSecrets":false}]}}
                """);
            await File.WriteAllTextAsync(Path.Combine(folder, ".confix.project"), "{}");
            await File.WriteAllTextAsync(Path.Combine(folder, ".confix.solution"), "{}");
            var input = Path.Combine(folder, "appsettings.json");
            var output = Path.Combine(folder, "rendered.json");
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"server\",\"Port\":5}}");
            var cli = Path.Combine(root, "src/Confix.Tool/src/Confix.Tool/bin/Debug/net10.0/Confix.dll");
            var valid = await Run(folder, cli, "build", "--output-file", output);
            valid.Exit.Should().Be(0, valid.Output);
            File.Exists(Path.Combine(folder, "application-started")).Should().BeFalse();
            File.Exists(Path.Combine(folder, "confix.ide.schema.json")).Should().BeFalse();
            var previous = await File.ReadAllTextAsync(output);
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"sensitive-marker\",\"Port\":99}}");
            var invalid = await Run(folder, cli, "build", "--output-file", output);
            invalid.Exit.Should().NotBe(0);
            invalid.Output.Should().Contain("Mail:Port").And.NotContain("sensitive-marker");
            (await File.ReadAllTextAsync(output)).Should().Be(previous);
            // Both configurations are present; Release must not accidentally execute Debug contracts.
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"server\",\"Port\":25}}");
            var rcPath = Path.Combine(folder, ".confixrc");
            var rc = JsonNode.Parse(await File.ReadAllTextAsync(rcPath))!;
            rc["project"]!["exportSchema"] = true;
            await File.WriteAllTextAsync(rcPath, rc.ToJsonString());
            var release = await Run(folder, cli, "build", "--output-file", output, "--dotnet-configuration", "Release");
            release.Exit.Should().Be(0, release.Output);
            var schema = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(folder, "confix.ide.schema.json")));
            schema!["properties"]!["Mail"].Should().NotBeNull();
            File.Exists(Path.Combine(folder, ".vscode/settings.json")).Should().BeTrue();
            File.Exists(Path.Combine(folder, "application-started")).Should().BeFalse();
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    private static string FindRoot([CallerFilePath] string source = "")
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(source)!);
        while (!File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props"))) directory = directory.Parent!;
        return directory.FullName;
    }

    private static async Task<(int Exit, string Output)> Run(string directory, params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = directory, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        using var kill = timeout.Token.Register(() => { try { process.Kill(true); } catch (InvalidOperationException) { } });
        await process.WaitForExitAsync(timeout.Token);
        return (process.ExitCode, await output + await error);
    }
}
