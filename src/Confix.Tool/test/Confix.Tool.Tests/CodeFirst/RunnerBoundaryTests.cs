using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using FluentAssertions;

namespace Confix.CodeFirst.Tests;

[Trait("Category", "EndToEnd")]
public sealed class RunnerBoundaryTests
{
    [Fact]
    public async Task CliUsesActualContractsWithoutStartingApplicationAndPreservesFailedOutput()
    {
        var root = FindRoot();
        var configurationJson = PackageVersion(root, "Microsoft.Extensions.Configuration.Json");
        var dependencyInjection = PackageVersion(root, "Microsoft.Extensions.DependencyInjection");
        var folder = Path.Combine(Path.GetTempPath(), "confix-boundary-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(folder, "Host.csproj"), $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>{TargetFramework}</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="{root}/src/Confix.Options/Confix.Options.csproj" />
                    <ProjectReference Include="{root}/src/Confix.CodeGeneration/Confix.CodeGeneration.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
                    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="{configurationJson}" />
                    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="{dependencyInjection}" />
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
            var cli = Path.Combine(
                root,
                "src/Confix.Tool/src/Confix.Tool/bin",
                BuildConfiguration,
                TargetFramework,
                "Confix.dll");

            File.Exists(cli).Should().BeTrue($"the CLI must be built at {cli}");

            // Valid configuration builds without ever running the application itself.
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"server\",\"Port\":5}}");

            var valid = await Run(folder, cli, "build", "--output-file", output);

            valid.Exit.Should().Be(0, valid.Output);
            File.Exists(Path.Combine(folder, "application-started")).Should().BeFalse();
            File.Exists(Path.Combine(folder, "confix.ide.schema.json")).Should().BeFalse();

            // A failing build reports the member path, leaks no values and keeps the old output.
            var previous = await File.ReadAllTextAsync(output);
            await File.WriteAllTextAsync(
                input,
                "{\"Mail\":{\"Host\":\"sensitive-marker\",\"Port\":99}}");

            var invalid = await Run(folder, cli, "build", "--output-file", output);

            invalid.Exit.Should().NotBe(0);
            invalid.Output.Should().Contain("Mail:Port").And.NotContain("sensitive-marker");
            (await File.ReadAllTextAsync(output)).Should().Be(previous);

            // Both configurations are present; Release must not accidentally execute Debug contracts.
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"server\",\"Port\":25}}");
            await EnableSchemaExportAsync(folder);

            var release = await Run(
                folder,
                cli,
                "build",
                "--output-file", output,
                "--dotnet-configuration", "Release");

            release.Exit.Should().Be(0, release.Output);

            var exported = Path.Combine(folder, "confix.ide.schema.json");
            var schema = JsonNode.Parse(await File.ReadAllTextAsync(exported));

            schema!["properties"]!["Mail"].Should().NotBeNull();
            File.Exists(Path.Combine(folder, ".vscode/settings.json")).Should().BeTrue();
            File.Exists(Path.Combine(folder, "application-started")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task ValidateComposesOverlaysHonoursCoverageAndNeverWrites()
    {
        var root = FindRoot();
        var folder = Path.Combine(Path.GetTempPath(), "confix-validate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            await WriteHostAsync(root, folder, """
                [ConfixSection("Mail")]
                public sealed class Mail
                {
                    [Required] public string Host { get; set; } = "";
                    [Range(1, 65535)] public int Port { get; set; } = 587;
                }
                """);

            var input = Path.Combine(folder, "appsettings.json");
            var cli = CliPath(root);

            // The base file alone is incomplete; the overlay supplies the required host.
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Port\":25}}");
            await File.WriteAllTextAsync(
                Path.Combine(folder, "appsettings.prod.json"),
                "{\"Mail\":{\"Host\":\"server\"}}");
            await ConfigureAsync(folder, rc => rc["project"]!["validation"] = new JsonObject
            {
                ["type"] = "dotnet-options",
                ["overlays"] = new JsonArray("appsettings.{environment}.json")
            });

            var composed = await Run(folder, cli, "validate");

            composed.Exit.Should().Be(0, composed.Output);

            // validate must not publish anything, not even the sidecar output.
            Directory.EnumerateFiles(folder, "*.confix.json").Should().BeEmpty();
            (await File.ReadAllTextAsync(input)).Should().Be("{\"Mail\":{\"Port\":25}}");

            // A missing overlay is an error rather than a silent skip.
            File.Delete(Path.Combine(folder, "appsettings.prod.json"));

            var missing = await Run(folder, cli, "validate");

            missing.Exit.Should().NotBe(0);
            missing.Output.Should().Contain("appsettings.{environment}.json");

            // Strict coverage rejects a root nobody owns; registeredSections tolerates it.
            await File.WriteAllTextAsync(input, "{\"Mail\":{\"Host\":\"server\"},\"Serilog\":{\"a\":1}}");
            await ConfigureAsync(folder, rc => rc["project"]!["validation"] = new JsonObject
            {
                ["type"] = "dotnet-options"
            });

            var strict = await Run(folder, cli, "validate");

            strict.Exit.Should().NotBe(0);
            strict.Output.Should().Contain("Serilog");

            await ConfigureAsync(folder, rc => rc["project"]!["validation"] = new JsonObject
            {
                ["type"] = "dotnet-options",
                ["coverage"] = "registeredSections"
            });

            var relaxed = await Run(folder, cli, "validate");

            relaxed.Exit.Should().Be(0, relaxed.Output);

            // Required validation can never be bypassed.
            var skipped = await Run(folder, cli, "validate", "--no-restore");

            skipped.Exit.Should().NotBe(0);
            skipped.Output.Should().Contain("--no-restore");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task VariablesAreResolvedBeforeTheContractsSeeTheConfiguration()
    {
        var root = FindRoot();
        var folder = Path.Combine(Path.GetTempPath(), "confix-variables-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            await WriteHostAsync(root, folder, """
                [ConfixSection("Mail")]
                public sealed class Mail
                {
                    [Required, MinLength(4)] public string Host { get; set; } = "";
                }
                """);

            await ConfigureAsync(folder, rc => rc["project"]!["variableProviders"] = new JsonArray(
                new JsonObject
                {
                    ["name"] = "local",
                    ["type"] = "local",
                    ["path"] = "variables.json"
                }));

            await File.WriteAllTextAsync(
                Path.Combine(folder, "variables.json"),
                "{\"host\":\"resolved-server\"}");
            await File.WriteAllTextAsync(
                Path.Combine(folder, "appsettings.json"),
                "{\"Mail\":{\"Host\":\"$local:host\"}}");

            var output = Path.Combine(folder, "rendered.json");
            var result = await Run(folder, CliPath(root), "build", "--output-file", output);

            result.Exit.Should().Be(0, result.Output);

            var rendered = JsonNode.Parse(await File.ReadAllTextAsync(output))!;

            rendered["Mail"]!["Host"]!.GetValue<string>().Should().Be("resolved-server");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static async Task WriteHostAsync(string root, string folder, string contract)
    {
        var configurationJson = PackageVersion(root, "Microsoft.Extensions.Configuration.Json");
        var dependencyInjection = PackageVersion(root, "Microsoft.Extensions.DependencyInjection");

        await File.WriteAllTextAsync(Path.Combine(folder, "Host.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>{TargetFramework}</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="{root}/src/Confix.Options/Confix.Options.csproj" />
                <ProjectReference Include="{root}/src/Confix.CodeGeneration/Confix.CodeGeneration.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
                <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="{configurationJson}" />
                <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="{dependencyInjection}" />
              </ItemGroup>
            </Project>
            """);

        await File.WriteAllTextAsync(Path.Combine(folder, "Program.cs"), $$"""
            using System.ComponentModel.DataAnnotations;
            using Confix;
            using Microsoft.Extensions.Configuration;
            using Microsoft.Extensions.DependencyInjection;
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var services = new ServiceCollection();
            services.AddConfixOptions<Mail>(configuration);
            {{contract}}
            """);

        await File.WriteAllTextAsync(Path.Combine(folder, ".confixrc"), """
            {"isRoot":true,"project":{"validation":{"type":"dotnet-options"},
             "configurationFiles":[{"type":"appsettings","useUserSecrets":false}]}}
            """);
        await File.WriteAllTextAsync(Path.Combine(folder, ".confix.project"), "{}");
        await File.WriteAllTextAsync(Path.Combine(folder, ".confix.solution"), "{}");
    }

    private static async Task ConfigureAsync(string folder, Action<JsonNode> configure)
    {
        var path = Path.Combine(folder, ".confixrc");
        var settings = JsonNode.Parse(await File.ReadAllTextAsync(path))!;

        configure(settings);

        await File.WriteAllTextAsync(path, settings.ToJsonString());
    }

    private static string CliPath(string root)
    {
        var cli = Path.Combine(
            root,
            "src/Confix.Tool/src/Confix.Tool/bin",
            BuildConfiguration,
            TargetFramework,
            "Confix.dll");

        File.Exists(cli).Should().BeTrue($"the CLI must be built at {cli}");

        return cli;
    }

    private static async Task EnableSchemaExportAsync(string folder)
    {
        var path = Path.Combine(folder, ".confixrc");
        var settings = JsonNode.Parse(await File.ReadAllTextAsync(path))!;

        settings["project"]!["exportSchema"] = true;

        await File.WriteAllTextAsync(path, settings.ToJsonString());
    }

    // The test assembly lives in bin/<configuration>/<targetFramework>, which the host must match.
    private static string TargetFramework => new DirectoryInfo(AppContext.BaseDirectory).Name;

    private static string BuildConfiguration =>
        new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;

    private static string PackageVersion(string root, string package)
    {
        return XDocument.Load(Path.Combine(root, "Directory.Packages.props"))
            .Descendants("PackageVersion")
            .First(element => (string?)element.Attribute("Include") == package)
            .Attribute("Version")!
            .Value;
    }

    private static string FindRoot([CallerFilePath] string source = "")
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(source)!);

        while (!File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props")))
        {
            directory = directory.Parent!;
        }

        return directory.FullName;
    }

    private static async Task<(int Exit, string Output)> Run(
        string directory,
        params string[] args)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start)!;

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        using var kill = timeout.Token.Register(() => Stop(process));

        await process.WaitForExitAsync(timeout.Token);

        return (process.ExitCode, await output + await error);
    }

    private static void Stop(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // The process has already exited.
        }
    }
}
