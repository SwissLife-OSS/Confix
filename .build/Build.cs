using System.Buffers;
using System.CommandLine;
using System.Text.Json;
using Confix.Tool;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using static Nuke.CodeGeneration.CodeGenerator;

public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.GenerateTools);

    AbsolutePath ToolSpecfication => RootDirectory / "src" / "Confix.Tool" / "src" / "Confix.Nuke" /
        "Confix.Tool.json";

    Target GenerateTools => _ => _
        .Executes(() =>
        {
            var builder = new ConfixCommandLine();
            var commands = new List<CommandTask>();

            var queue = new Stack<string>();
            foreach (var subcommand in builder.Command.Subcommands)
            {
                Visit(subcommand, queue);
            }

            var buffer = new ArrayBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(buffer, new() { Indented = true });
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("$schema",
                "https://raw.githubusercontent.com/nuke-build/nuke/master/source/Nuke.CodeGeneration/schema.json");
            utf8JsonWriter.WriteString("name", "Confix");
            utf8JsonWriter.WriteString("officialUrl", "https://swisslife-oss.github.io/Confix/");
            utf8JsonWriter.WriteString("packageId", "Confix");
            utf8JsonWriter.WriteString("packageExecutable", "Confix.dll");
            utf8JsonWriter.WriteBoolean("customExecutable", true);
            utf8JsonWriter.WriteBoolean("customLogger", true);
            utf8JsonWriter.WriteStartArray("tasks");
            foreach (var command in commands)
            {
                utf8JsonWriter.WriteStartObject();
                utf8JsonWriter.WriteString("help", command.Command.Description);
                utf8JsonWriter.WriteString("postfix", command.Name);
                utf8JsonWriter.WriteString("definiteArgument", command.Argument);
                utf8JsonWriter.WriteStartObject("settingsClass");
                utf8JsonWriter.WriteStartArray("properties");
                foreach (var property in command.Command.Options)
                {
                    utf8JsonWriter.WriteStartObject();
                    utf8JsonWriter.WriteString("name", EncodeName(property.Name));
                    utf8JsonWriter.WriteString("type", "string");
                    utf8JsonWriter.WriteString("format", $"--{property.Name} {{value}}");
                    utf8JsonWriter.WriteString("help", property.Description);
                    utf8JsonWriter.WriteEndObject();
                }

                foreach (var property in command.Command.Arguments)
                {
                    utf8JsonWriter.WriteStartObject();
                    utf8JsonWriter.WriteString("name", EncodeName(property.Name));
                    utf8JsonWriter.WriteString("type", "string");
                    utf8JsonWriter.WriteString("format", "{value}");
                    utf8JsonWriter.WriteString("help", property.Description);
                    utf8JsonWriter.WriteEndObject();
                }

                utf8JsonWriter.WriteEndArray();
                utf8JsonWriter.WriteEndObject();
                utf8JsonWriter.WriteEndObject();
            }

            utf8JsonWriter.WriteEndArray();

            utf8JsonWriter.WriteStartArray("commonTaskProperties");
            utf8JsonWriter.WriteStartObject();
            utf8JsonWriter.WriteString("name", "Framework");
            utf8JsonWriter.WriteString("type", "string");
            utf8JsonWriter.WriteString("noArgument", "true");
            utf8JsonWriter.WriteEndObject();
            utf8JsonWriter.WriteEndArray();
            utf8JsonWriter.WriteEndObject();

            utf8JsonWriter.Flush();
            utf8JsonWriter.Dispose();

            File.Delete(ToolSpecfication);
            using (var file = File.Create(ToolSpecfication))
            {
                file.Write(buffer.WrittenSpan);
                file.Flush();
            }

            GenerateCode(ToolSpecfication, namespaceProvider: _ => "Confix.Nuke");

            void Visit(Command command, Stack<string> parents)
            {
                parents.Push(command.Name);

                if (command.Subcommands.Count == 0)
                {
                    var reversed = parents.Reverse().ToList();
                    var name = string.Join("", reversed.Select(Capitalize));
                    commands.Add(new(name, string.Join(" ", reversed), command));
                }
                else
                {
                    foreach (var subcommand in command.Subcommands)
                    {
                        Visit(subcommand, parents);
                    }
                }

                parents.Pop();
            }
        });

    private static string EncodeName(string name)
    {
        name = name.Replace("--", "");
        return string.Join("", name.Split("-").Select(Capitalize));
    }

    private static string Capitalize(string name)
    {
        return name[..1].ToUpper() + name[1..];
    }
}

/*
{
  "$schema": "https://raw.githubusercontent.com/nuke-build/nuke/master/source/Nuke.CodeGeneration/schema.json",
  "name": "Confix",
  "officialUrl": "https://swisslife-oss.github.io/Confix/",
  "packageId": "Confix",
  "packageExecutable": "Confix.dll",
  "customExecutable": true,
  "customLogger": true,
  "tasks": [
    {
      "help": "",
      "postfix": "ComponentBuild",
      "definiteArgument": "component build",
      "settingsClass": {
        "properties": [
          {
            "name": "Verbosity",
            "type": "string",
            "format": "--verbosity {value}",
            "help": "Sets the verbosity level <Detailed|Diagnostic|Minimal|Normal|Quiet> [default: Normal]"
          }
        ]
      }
    },
    {
      "help": "",
      "postfix": "ComponentInit",
      "definiteArgument": "component init",
      "settingsClass": {
        "properties": [
          {
            "name": "Name",
            "type": "string",
            "format": "{value}",
            "help": "The name of the component"
          },
          {
            "name": "Verbosity",
            "type": "string",
            "format": "--verbosity {value}",
            "help": "Sets the verbosity level <Detailed|Diagnostic|Minimal|Normal|Quiet> [default: Normal]"
          }
        ]
      }
    },
    {
      "help": "",
      "postfix": "ProjectReload",
      "definiteArgument": "project reload",
      "settingsClass": {
        "properties": [
          {
            "name": "Environment",
            "type": "string",
            "format": "--environment {value}",
            "help": "The name of the environment to run the command in. Overrules the active environment set in .confixrc"
          },
          {
            "name": "OutputFile",
            "type": "string",
            "format": "--output-file {value}",
            "help": "specifies the output file"
          },
          {
            "name": "Verbosity",
            "type": "string",
            "format": "--verbosity {value}",
            "help": "Sets the verbosity level <Detailed|Diagnostic|Minimal|Normal|Quiet> [default: Normal]"
          }
        ]
      }
    },
    }
  ],
  "commonTaskProperties": [
    {
      "name": "Framework",
      "type": "string",
      "noArgument": true
    }
  ]
}
*/

public record CommandTask(string Name, string Argument, Command Command);
