
using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Confix.Nuke;

/// <summary>
///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public partial class ConfixTasks
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public static string ConfixPath =>
        ToolPathResolver.TryGetEnvironmentExecutable("CONFIX_EXE") ??
        GetToolPath();
    public static Action<OutputType, string> ConfixLogger { get; set; } = CustomLogger;
    public static Action<ToolSettings, IProcess> ConfixExitHandler { get; set; } = ProcessTasks.DefaultExitHandler;
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    public static IReadOnlyCollection<Output> Confix(ArgumentStringHandler arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Action<OutputType, string> logger = null, Action<IProcess> exitHandler = null)
    {
        using var process = ProcessTasks.StartProcess(ConfixPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, logger ?? ConfixLogger);
        (exitHandler ?? (p => ConfixExitHandler.Invoke(null, p))).Invoke(process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>builds a component. Runs all configured component inputs</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentBuild(ConfixComponentBuildSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixComponentBuildSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>builds a component. Runs all configured component inputs</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentBuild(Configure<ConfixComponentBuildSettings> configurator)
    {
        return ConfixComponentBuild(configurator(new ConfixComponentBuildSettings()));
    }
    /// <summary>
    ///   <p>builds a component. Runs all configured component inputs</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixComponentBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentBuild(CombinatorialConfigure<ConfixComponentBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixComponentBuild, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Initializes a component and creates a component file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentInit(ConfixComponentInitSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixComponentInitSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Initializes a component and creates a component file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentInit(Configure<ConfixComponentInitSettings> configurator)
    {
        return ConfixComponentInit(configurator(new ConfixComponentInitSettings()));
    }
    /// <summary>
    ///   <p>Initializes a component and creates a component file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixComponentInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentInit(CombinatorialConfigure<ConfixComponentInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixComponentInit, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Lists the component of the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentList(ConfixComponentListSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixComponentListSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Lists the component of the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentList(Configure<ConfixComponentListSettings> configurator)
    {
        return ConfixComponentList(configurator(new ConfixComponentListSettings()));
    }
    /// <summary>
    ///   <p>Lists the component of the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixComponentListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentList(CombinatorialConfigure<ConfixComponentListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixComponentList, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Adds a component to the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li>
    ///     <li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentAdd(ConfixComponentAddSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixComponentAddSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Adds a component to the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li>
    ///     <li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixComponentAdd(Configure<ConfixComponentAddSettings> configurator)
    {
        return ConfixComponentAdd(configurator(new ConfixComponentAddSettings()));
    }
    /// <summary>
    ///   <p>Adds a component to the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li>
    ///     <li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixComponentAddSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentAdd(CombinatorialConfigure<ConfixComponentAddSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixComponentAdd, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Reloads the schema of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectRestore(ConfixProjectRestoreSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixProjectRestoreSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Reloads the schema of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectRestore(Configure<ConfixProjectRestoreSettings> configurator)
    {
        return ConfixProjectRestore(configurator(new ConfixProjectRestoreSettings()));
    }
    /// <summary>
    ///   <p>Reloads the schema of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixProjectRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectRestore(CombinatorialConfigure<ConfixProjectRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixProjectRestore, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Replaces all variables in the project files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectBuild(ConfixProjectBuildSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixProjectBuildSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Replaces all variables in the project files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectBuild(Configure<ConfixProjectBuildSettings> configurator)
    {
        return ConfixProjectBuild(configurator(new ConfixProjectBuildSettings()));
    }
    /// <summary>
    ///   <p>Replaces all variables in the project files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixProjectBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectBuild(CombinatorialConfigure<ConfixProjectBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixProjectBuild, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Initializes a project and creates a project file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectInit(ConfixProjectInitSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixProjectInitSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Initializes a project and creates a project file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectInit(Configure<ConfixProjectInitSettings> configurator)
    {
        return ConfixProjectInit(configurator(new ConfixProjectInitSettings()));
    }
    /// <summary>
    ///   <p>Initializes a project and creates a project file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixProjectInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectInit(CombinatorialConfigure<ConfixProjectInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixProjectInit, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Validates the configuration files of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectValidate(ConfixProjectValidateSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixProjectValidateSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Validates the configuration files of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectValidate(Configure<ConfixProjectValidateSettings> configurator)
    {
        return ConfixProjectValidate(configurator(new ConfixProjectValidateSettings()));
    }
    /// <summary>
    ///   <p>Validates the configuration files of a project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixProjectValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectValidate(CombinatorialConfigure<ConfixProjectValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixProjectValidate, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Generates a report for the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectReport(ConfixProjectReportSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixProjectReportSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Generates a report for the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixProjectReport(Configure<ConfixProjectReportSettings> configurator)
    {
        return ConfixProjectReport(configurator(new ConfixProjectReportSettings()));
    }
    /// <summary>
    ///   <p>Generates a report for the project</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li>
    ///     <li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li>
    ///     <li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixProjectReportSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectReport(CombinatorialConfigure<ConfixProjectReportSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixProjectReport, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Reloads the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionRestore(ConfixSolutionRestoreSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixSolutionRestoreSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Reloads the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionRestore(Configure<ConfixSolutionRestoreSettings> configurator)
    {
        return ConfixSolutionRestore(configurator(new ConfixSolutionRestoreSettings()));
    }
    /// <summary>
    ///   <p>Reloads the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixSolutionRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionRestore(CombinatorialConfigure<ConfixSolutionRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixSolutionRestore, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Replaces all variables in the solution files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionBuild(ConfixSolutionBuildSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixSolutionBuildSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Replaces all variables in the solution files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionBuild(Configure<ConfixSolutionBuildSettings> configurator)
    {
        return ConfixSolutionBuild(configurator(new ConfixSolutionBuildSettings()));
    }
    /// <summary>
    ///   <p>Replaces all variables in the solution files with their values</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixSolutionBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionBuild(CombinatorialConfigure<ConfixSolutionBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixSolutionBuild, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Initializes a solution and creates a solution file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionInit(ConfixSolutionInitSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixSolutionInitSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Initializes a solution and creates a solution file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionInit(Configure<ConfixSolutionInitSettings> configurator)
    {
        return ConfixSolutionInit(configurator(new ConfixSolutionInitSettings()));
    }
    /// <summary>
    ///   <p>Initializes a solution and creates a solution file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixSolutionInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionInit(CombinatorialConfigure<ConfixSolutionInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixSolutionInit, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionValidate(ConfixSolutionValidateSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixSolutionValidateSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionValidate(Configure<ConfixSolutionValidateSettings> configurator)
    {
        return ConfixSolutionValidate(configurator(new ConfixSolutionValidateSettings()));
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects in the solution</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixSolutionValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionValidate(CombinatorialConfigure<ConfixSolutionValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixSolutionValidate, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>resolves a variable by name</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableGet(ConfixVariableGetSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixVariableGetSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>resolves a variable by name</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableGet(Configure<ConfixVariableGetSettings> configurator)
    {
        return ConfixVariableGet(configurator(new ConfixVariableGetSettings()));
    }
    /// <summary>
    ///   <p>resolves a variable by name</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li>
    ///     <li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixVariableGetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableGet(CombinatorialConfigure<ConfixVariableGetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixVariableGet, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>sets a variable. Overrides existing value if any.</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li>
    ///     <li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableSet(ConfixVariableSetSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixVariableSetSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>sets a variable. Overrides existing value if any.</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li>
    ///     <li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableSet(Configure<ConfixVariableSetSettings> configurator)
    {
        return ConfixVariableSet(configurator(new ConfixVariableSetSettings()));
    }
    /// <summary>
    ///   <p>sets a variable. Overrides existing value if any.</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li>
    ///     <li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li>
    ///     <li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixVariableSetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableSet(CombinatorialConfigure<ConfixVariableSetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixVariableSet, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>list available variables</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li>
    ///     <li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableList(ConfixVariableListSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixVariableListSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>list available variables</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li>
    ///     <li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableList(Configure<ConfixVariableListSettings> configurator)
    {
        return ConfixVariableList(configurator(new ConfixVariableListSettings()));
    }
    /// <summary>
    ///   <p>list available variables</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li>
    ///     <li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixVariableListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableList(CombinatorialConfigure<ConfixVariableListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixVariableList, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Copies a variable from one provider to another provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li>
    ///     <li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li>
    ///     <li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li>
    ///     <li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableCopy(ConfixVariableCopySettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixVariableCopySettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Copies a variable from one provider to another provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li>
    ///     <li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li>
    ///     <li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li>
    ///     <li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixVariableCopy(Configure<ConfixVariableCopySettings> configurator)
    {
        return ConfixVariableCopy(configurator(new ConfixVariableCopySettings()));
    }
    /// <summary>
    ///   <p>Copies a variable from one provider to another provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li>
    ///     <li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li>
    ///     <li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li>
    ///     <li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixVariableCopySettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableCopy(CombinatorialConfigure<ConfixVariableCopySettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixVariableCopy, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixBuild(ConfixBuildSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixBuildSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixBuild(Configure<ConfixBuildSettings> configurator)
    {
        return ConfixBuild(configurator(new ConfixBuildSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li>
    ///     <li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixBuild(CombinatorialConfigure<ConfixBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixBuild, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixRestore(ConfixRestoreSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixRestoreSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixRestore(Configure<ConfixRestoreSettings> configurator)
    {
        return ConfixRestore(configurator(new ConfixRestoreSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixRestore(CombinatorialConfigure<ConfixRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixRestore, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixValidate(ConfixValidateSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixValidateSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixValidate(Configure<ConfixValidateSettings> configurator)
    {
        return ConfixValidate(configurator(new ConfixValidateSettings()));
    }
    /// <summary>
    ///   <p>Validates the schema of all the projects</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixValidate(CombinatorialConfigure<ConfixValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixValidate, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Encrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixEncrypt(ConfixEncryptSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixEncryptSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Encrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixEncrypt(Configure<ConfixEncryptSettings> configurator)
    {
        return ConfixEncrypt(configurator(new ConfixEncryptSettings()));
    }
    /// <summary>
    ///   <p>Encrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixEncryptSettings Settings, IReadOnlyCollection<Output> Output)> ConfixEncrypt(CombinatorialConfigure<ConfixEncryptSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixEncrypt, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Decrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixDecrypt(ConfixDecryptSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixDecryptSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Decrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixDecrypt(Configure<ConfixDecryptSettings> configurator)
    {
        return ConfixDecrypt(configurator(new ConfixDecryptSettings()));
    }
    /// <summary>
    ///   <p>Decrypts a file using the configured provider</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li>
    ///     <li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li>
    ///     <li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixDecryptSettings Settings, IReadOnlyCollection<Output> Output)> ConfixDecrypt(CombinatorialConfigure<ConfixDecryptSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixDecrypt, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Shows the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigShow(ConfixConfigShowSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixConfigShowSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Shows the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigShow(Configure<ConfixConfigShowSettings> configurator)
    {
        return ConfixConfigShow(configurator(new ConfixConfigShowSettings()));
    }
    /// <summary>
    ///   <p>Shows the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixConfigShowSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigShow(CombinatorialConfigure<ConfixConfigShowSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixConfigShow, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Sets a configuration value in the nearest .confixrc</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li>
    ///     <li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigSet(ConfixConfigSetSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixConfigSetSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Sets a configuration value in the nearest .confixrc</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li>
    ///     <li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigSet(Configure<ConfixConfigSetSettings> configurator)
    {
        return ConfixConfigSet(configurator(new ConfixConfigSetSettings()));
    }
    /// <summary>
    ///   <p>Sets a configuration value in the nearest .confixrc</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li>
    ///     <li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixConfigSetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigSet(CombinatorialConfigure<ConfixConfigSetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixConfigSet, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>Lists the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigList(ConfixConfigListSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new ConfixConfigListSettings();
        using var process = ProcessTasks.StartProcess(toolSettings);
        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());
        return process.Output;
    }
    /// <summary>
    ///   <p>Lists the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> ConfixConfigList(Configure<ConfixConfigListSettings> configurator)
    {
        return ConfixConfigList(configurator(new ConfixConfigListSettings()));
    }
    /// <summary>
    ///   <p>Lists the configuration to a file</p>
    ///   <p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li>
    ///     <li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(ConfixConfigListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigList(CombinatorialConfigure<ConfixConfigListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(ConfixConfigList, ConfixLogger, degreeOfParallelism, completeOnFailure);
    }
}
#region ConfixComponentBuildSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixComponentBuildSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("component build")
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixComponentInitSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixComponentInitSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    /// <summary>
    ///   The name of the component
    /// </summary>
    public virtual string Name { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("component init")
          .Add("--verbosity {value}", Verbosity)
          .Add("{value}", Name);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixComponentListSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixComponentListSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the output format
    /// </summary>
    public virtual string Format { get; internal set; }
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   If you specify this option, only the components will be built.
    /// </summary>
    public virtual string OnlyComponents { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("component list")
          .Add("--format {value}", Format)
          .Add("--output-file {value}", OutputFile)
          .Add("--environment {value}", Environment)
          .Add("--only-components {value}", OnlyComponents)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixComponentAddSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixComponentAddSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Shows the version information
    /// </summary>
    public virtual string Version { get; internal set; }
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    /// <summary>
    ///   The name of the component
    /// </summary>
    public virtual string Name { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("component add")
          .Add("--version {value}", Version)
          .Add("--output-file {value}", OutputFile)
          .Add("--verbosity {value}", Verbosity)
          .Add("{value}", Name);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixProjectRestoreSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixProjectRestoreSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   If you specify this option, only the components will be built.
    /// </summary>
    public virtual string OnlyComponents { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("project restore")
          .Add("--output-file {value}", OutputFile)
          .Add("--environment {value}", Environment)
          .Add("--only-components {value}", OnlyComponents)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixProjectBuildSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixProjectBuildSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Disables restoring of schemas
    /// </summary>
    public virtual string NoRestore { get; internal set; }
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   If you specify this option, only the components will be built.
    /// </summary>
    public virtual string OnlyComponents { get; internal set; }
    /// <summary>
    ///   Encrypt the output file
    /// </summary>
    public virtual string Encrypt { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("project build")
          .Add("--no-restore {value}", NoRestore)
          .Add("--output-file {value}", OutputFile)
          .Add("--environment {value}", Environment)
          .Add("--only-components {value}", OnlyComponents)
          .Add("--encrypt {value}", Encrypt)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixProjectInitSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixProjectInitSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("project init")
          .Add("--output-file {value}", OutputFile)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixProjectValidateSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixProjectValidateSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   If you specify this option, only the components will be built.
    /// </summary>
    public virtual string OnlyComponents { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("project validate")
          .Add("--output-file {value}", OutputFile)
          .Add("--environment {value}", Environment)
          .Add("--only-components {value}", OnlyComponents)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixProjectReportSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixProjectReportSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Disables restoring of schemas
    /// </summary>
    public virtual string NoRestore { get; internal set; }
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   The path to the report file. If not specified, the report will be written to the console.
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   If you specify this option, only the components will be built.
    /// </summary>
    public virtual string OnlyComponents { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("project report")
          .Add("--no-restore {value}", NoRestore)
          .Add("--environment {value}", Environment)
          .Add("--output-file {value}", OutputFile)
          .Add("--only-components {value}", OnlyComponents)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixSolutionRestoreSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixSolutionRestoreSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("solution restore")
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixSolutionBuildSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixSolutionBuildSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("solution build")
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixSolutionInitSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixSolutionInitSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("solution init")
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixSolutionValidateSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixSolutionValidateSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("solution validate")
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixVariableGetSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixVariableGetSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   The name of the variable
    /// </summary>
    public virtual string Name { get; internal set; }
    /// <summary>
    ///   Sets the output format
    /// </summary>
    public virtual string Format { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("variable get")
          .Add("--environment {value}", Environment)
          .Add("--name {value}", Name)
          .Add("--format {value}", Format)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixVariableSetSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixVariableSetSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   The name of the variable
    /// </summary>
    public virtual string Name { get; internal set; }
    /// <summary>
    ///   The value of the variable
    /// </summary>
    public virtual string Value { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("variable set")
          .Add("--environment {value}", Environment)
          .Add("--name {value}", Name)
          .Add("--value {value}", Value)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixVariableListSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixVariableListSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   The name of the provider to resolve the variable from
    /// </summary>
    public virtual string Provider { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("variable list")
          .Add("--environment {value}", Environment)
          .Add("--provider {value}", Provider)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixVariableCopySettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixVariableCopySettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   The name of the new variable
    /// </summary>
    public virtual string From { get; internal set; }
    /// <summary>
    ///   The name of the new variable
    /// </summary>
    public virtual string To { get; internal set; }
    /// <summary>
    ///   The name of the environment you want to migrate the variable to
    /// </summary>
    public virtual string ToEnvironment { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("variable copy")
          .Add("--environment {value}", Environment)
          .Add("--from {value}", From)
          .Add("--to {value}", To)
          .Add("--to-environment {value}", ToEnvironment)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixBuildSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixBuildSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   Specifies the output file
    /// </summary>
    public virtual string OutputFile { get; internal set; }
    /// <summary>
    ///   Encrypt the output file
    /// </summary>
    public virtual string Encrypt { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("build")
          .Add("--environment {value}", Environment)
          .Add("--output-file {value}", OutputFile)
          .Add("--encrypt {value}", Encrypt)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixRestoreSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixRestoreSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("restore")
          .Add("--environment {value}", Environment)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixValidateSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixValidateSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("validate")
          .Add("--environment {value}", Environment)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixEncryptSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixEncryptSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    /// <summary>
    ///   The path to the file to encrypt or decrypt.
    /// </summary>
    public virtual string InputFile { get; internal set; }
    /// <summary>
    ///   The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.
    /// </summary>
    public virtual string OutFile { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("encrypt")
          .Add("--environment {value}", Environment)
          .Add("--verbosity {value}", Verbosity)
          .Add("{value}", InputFile)
          .Add("{value}", OutFile);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixDecryptSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixDecryptSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   The name of the environment to run the command in. Overrules the active environment set in .confixrc
    /// </summary>
    public virtual string Environment { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    /// <summary>
    ///   The path to the file to encrypt or decrypt.
    /// </summary>
    public virtual string InputFile { get; internal set; }
    /// <summary>
    ///   The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.
    /// </summary>
    public virtual string OutFile { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("decrypt")
          .Add("--environment {value}", Environment)
          .Add("--verbosity {value}", Verbosity)
          .Add("{value}", InputFile)
          .Add("{value}", OutFile);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixConfigShowSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixConfigShowSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the output format
    /// </summary>
    public virtual string Format { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("config show")
          .Add("--format {value}", Format)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixConfigSetSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixConfigSetSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    /// <summary>
    ///   The path to the configuration file
    /// </summary>
    public virtual string Path { get; internal set; }
    /// <summary>
    ///   The value to set as json
    /// </summary>
    public virtual string Value { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("config set")
          .Add("--verbosity {value}", Verbosity)
          .Add("{value}", Path)
          .Add("{value}", Value);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixConfigListSettings
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class ConfixConfigListSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Confix executable.
    /// </summary>
    public override string ProcessToolPath => base.ProcessToolPath ?? GetProcessToolPath();
    public override Action<OutputType, string> ProcessLogger => base.ProcessLogger ?? ConfixTasks.ConfixLogger;
    public override Action<ToolSettings, IProcess> ProcessExitHandler => base.ProcessExitHandler ?? ConfixTasks.ConfixExitHandler;
    /// <summary>
    ///   Sets the output format
    /// </summary>
    public virtual string Format { get; internal set; }
    /// <summary>
    ///   Sets the verbosity level
    /// </summary>
    public virtual string Verbosity { get; internal set; }
    public virtual string Framework { get; internal set; }
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        arguments
          .Add("config list")
          .Add("--format {value}", Format)
          .Add("--verbosity {value}", Verbosity);
        return base.ConfigureProcessArguments(arguments);
    }
}
#endregion
#region ConfixComponentBuildSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentBuildSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixComponentBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixComponentBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixComponentBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixComponentBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixComponentInitSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentInitSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Name
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentInitSettings.Name"/></em></p>
    ///   <p>The name of the component</p>
    /// </summary>
    [Pure]
    public static T SetName<T>(this T toolSettings, string name) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = name;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentInitSettings.Name"/></em></p>
    ///   <p>The name of the component</p>
    /// </summary>
    [Pure]
    public static T ResetName<T>(this T toolSettings) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixComponentInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixComponentListSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentListSettingsExtensions
{
    #region Format
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T SetFormat<T>(this T toolSettings, string format) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = format;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T ResetFormat<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = null;
        return toolSettings;
    }
    #endregion
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OnlyComponents
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T SetOnlyComponents<T>(this T toolSettings, string onlyComponents) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = onlyComponents;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T ResetOnlyComponents<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixComponentListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixComponentAddSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentAddSettingsExtensions
{
    #region Version
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentAddSettings.Version"/></em></p>
    ///   <p>Shows the version information</p>
    /// </summary>
    [Pure]
    public static T SetVersion<T>(this T toolSettings, string version) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Version = version;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentAddSettings.Version"/></em></p>
    ///   <p>Shows the version information</p>
    /// </summary>
    [Pure]
    public static T ResetVersion<T>(this T toolSettings) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Version = null;
        return toolSettings;
    }
    #endregion
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentAddSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentAddSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentAddSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentAddSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Name
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentAddSettings.Name"/></em></p>
    ///   <p>The name of the component</p>
    /// </summary>
    [Pure]
    public static T SetName<T>(this T toolSettings, string name) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = name;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentAddSettings.Name"/></em></p>
    ///   <p>The name of the component</p>
    /// </summary>
    [Pure]
    public static T ResetName<T>(this T toolSettings) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixComponentAddSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixComponentAddSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixComponentAddSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixProjectRestoreSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectRestoreSettingsExtensions
{
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectRestoreSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectRestoreSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectRestoreSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectRestoreSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OnlyComponents
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T SetOnlyComponents<T>(this T toolSettings, string onlyComponents) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = onlyComponents;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T ResetOnlyComponents<T>(this T toolSettings) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixProjectRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixProjectBuildSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectBuildSettingsExtensions
{
    #region NoRestore
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.NoRestore"/></em></p>
    ///   <p>Disables restoring of schemas</p>
    /// </summary>
    [Pure]
    public static T SetNoRestore<T>(this T toolSettings, string noRestore) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.NoRestore = noRestore;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.NoRestore"/></em></p>
    ///   <p>Disables restoring of schemas</p>
    /// </summary>
    [Pure]
    public static T ResetNoRestore<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.NoRestore = null;
        return toolSettings;
    }
    #endregion
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OnlyComponents
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T SetOnlyComponents<T>(this T toolSettings, string onlyComponents) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = onlyComponents;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T ResetOnlyComponents<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = null;
        return toolSettings;
    }
    #endregion
    #region Encrypt
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.Encrypt"/></em></p>
    ///   <p>Encrypt the output file</p>
    /// </summary>
    [Pure]
    public static T SetEncrypt<T>(this T toolSettings, string encrypt) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Encrypt = encrypt;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.Encrypt"/></em></p>
    ///   <p>Encrypt the output file</p>
    /// </summary>
    [Pure]
    public static T ResetEncrypt<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Encrypt = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixProjectBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixProjectInitSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectInitSettingsExtensions
{
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectInitSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectInitSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixProjectInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixProjectValidateSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectValidateSettingsExtensions
{
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectValidateSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectValidateSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectValidateSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectValidateSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OnlyComponents
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectValidateSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T SetOnlyComponents<T>(this T toolSettings, string onlyComponents) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = onlyComponents;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectValidateSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T ResetOnlyComponents<T>(this T toolSettings) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixProjectValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixProjectReportSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectReportSettingsExtensions
{
    #region NoRestore
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.NoRestore"/></em></p>
    ///   <p>Disables restoring of schemas</p>
    /// </summary>
    [Pure]
    public static T SetNoRestore<T>(this T toolSettings, string noRestore) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.NoRestore = noRestore;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.NoRestore"/></em></p>
    ///   <p>Disables restoring of schemas</p>
    /// </summary>
    [Pure]
    public static T ResetNoRestore<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.NoRestore = null;
        return toolSettings;
    }
    #endregion
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.OutputFile"/></em></p>
    ///   <p>The path to the report file. If not specified, the report will be written to the console.</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.OutputFile"/></em></p>
    ///   <p>The path to the report file. If not specified, the report will be written to the console.</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region OnlyComponents
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T SetOnlyComponents<T>(this T toolSettings, string onlyComponents) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = onlyComponents;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.OnlyComponents"/></em></p>
    ///   <p>If you specify this option, only the components will be built.</p>
    /// </summary>
    [Pure]
    public static T ResetOnlyComponents<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OnlyComponents = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixProjectReportSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixProjectReportSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixProjectReportSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixSolutionRestoreSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionRestoreSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixSolutionRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixSolutionRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixSolutionRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixSolutionRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixSolutionBuildSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionBuildSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixSolutionBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixSolutionBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixSolutionBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixSolutionBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixSolutionInitSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionInitSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixSolutionInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionInitSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixSolutionInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixSolutionInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionInitSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixSolutionInitSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixSolutionValidateSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionValidateSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixSolutionValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixSolutionValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixSolutionValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixSolutionValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixSolutionValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixSolutionValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixVariableGetSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableGetSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableGetSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableGetSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Name
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableGetSettings.Name"/></em></p>
    ///   <p>The name of the variable</p>
    /// </summary>
    [Pure]
    public static T SetName<T>(this T toolSettings, string name) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = name;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableGetSettings.Name"/></em></p>
    ///   <p>The name of the variable</p>
    /// </summary>
    [Pure]
    public static T ResetName<T>(this T toolSettings) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = null;
        return toolSettings;
    }
    #endregion
    #region Format
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableGetSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T SetFormat<T>(this T toolSettings, string format) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = format;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableGetSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T ResetFormat<T>(this T toolSettings) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableGetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableGetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableGetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableGetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixVariableGetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixVariableSetSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableSetSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableSetSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableSetSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Name
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableSetSettings.Name"/></em></p>
    ///   <p>The name of the variable</p>
    /// </summary>
    [Pure]
    public static T SetName<T>(this T toolSettings, string name) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = name;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableSetSettings.Name"/></em></p>
    ///   <p>The name of the variable</p>
    /// </summary>
    [Pure]
    public static T ResetName<T>(this T toolSettings) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Name = null;
        return toolSettings;
    }
    #endregion
    #region Value
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableSetSettings.Value"/></em></p>
    ///   <p>The value of the variable</p>
    /// </summary>
    [Pure]
    public static T SetValue<T>(this T toolSettings, string value) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Value = value;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableSetSettings.Value"/></em></p>
    ///   <p>The value of the variable</p>
    /// </summary>
    [Pure]
    public static T ResetValue<T>(this T toolSettings) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Value = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableSetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableSetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableSetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableSetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixVariableSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixVariableListSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableListSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableListSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableListSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Provider
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableListSettings.Provider"/></em></p>
    ///   <p>The name of the provider to resolve the variable from</p>
    /// </summary>
    [Pure]
    public static T SetProvider<T>(this T toolSettings, string provider) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Provider = provider;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableListSettings.Provider"/></em></p>
    ///   <p>The name of the provider to resolve the variable from</p>
    /// </summary>
    [Pure]
    public static T ResetProvider<T>(this T toolSettings) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Provider = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixVariableListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixVariableCopySettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableCopySettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region From
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.From"/></em></p>
    ///   <p>The name of the new variable</p>
    /// </summary>
    [Pure]
    public static T SetFrom<T>(this T toolSettings, string from) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.From = from;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.From"/></em></p>
    ///   <p>The name of the new variable</p>
    /// </summary>
    [Pure]
    public static T ResetFrom<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.From = null;
        return toolSettings;
    }
    #endregion
    #region To
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.To"/></em></p>
    ///   <p>The name of the new variable</p>
    /// </summary>
    [Pure]
    public static T SetTo<T>(this T toolSettings, string to) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.To = to;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.To"/></em></p>
    ///   <p>The name of the new variable</p>
    /// </summary>
    [Pure]
    public static T ResetTo<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.To = null;
        return toolSettings;
    }
    #endregion
    #region ToEnvironment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.ToEnvironment"/></em></p>
    ///   <p>The name of the environment you want to migrate the variable to</p>
    /// </summary>
    [Pure]
    public static T SetToEnvironment<T>(this T toolSettings, string toEnvironment) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ToEnvironment = toEnvironment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.ToEnvironment"/></em></p>
    ///   <p>The name of the environment you want to migrate the variable to</p>
    /// </summary>
    [Pure]
    public static T ResetToEnvironment<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ToEnvironment = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixVariableCopySettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixVariableCopySettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixVariableCopySettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixBuildSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixBuildSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixBuildSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixBuildSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region OutputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixBuildSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T SetOutputFile<T>(this T toolSettings, string outputFile) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = outputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixBuildSettings.OutputFile"/></em></p>
    ///   <p>Specifies the output file</p>
    /// </summary>
    [Pure]
    public static T ResetOutputFile<T>(this T toolSettings) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputFile = null;
        return toolSettings;
    }
    #endregion
    #region Encrypt
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixBuildSettings.Encrypt"/></em></p>
    ///   <p>Encrypt the output file</p>
    /// </summary>
    [Pure]
    public static T SetEncrypt<T>(this T toolSettings, string encrypt) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Encrypt = encrypt;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixBuildSettings.Encrypt"/></em></p>
    ///   <p>Encrypt the output file</p>
    /// </summary>
    [Pure]
    public static T ResetEncrypt<T>(this T toolSettings) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Encrypt = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixBuildSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixBuildSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixBuildSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixRestoreSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixRestoreSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixRestoreSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixRestoreSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixRestoreSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixRestoreSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixValidateSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixValidateSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixValidateSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixValidateSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixValidateSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixValidateSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixValidateSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixEncryptSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixEncryptSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixEncryptSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixEncryptSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixEncryptSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixEncryptSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region InputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixEncryptSettings.InputFile"/></em></p>
    ///   <p>The path to the file to encrypt or decrypt.</p>
    /// </summary>
    [Pure]
    public static T SetInputFile<T>(this T toolSettings, string inputFile) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.InputFile = inputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixEncryptSettings.InputFile"/></em></p>
    ///   <p>The path to the file to encrypt or decrypt.</p>
    /// </summary>
    [Pure]
    public static T ResetInputFile<T>(this T toolSettings) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.InputFile = null;
        return toolSettings;
    }
    #endregion
    #region OutFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixEncryptSettings.OutFile"/></em></p>
    ///   <p>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</p>
    /// </summary>
    [Pure]
    public static T SetOutFile<T>(this T toolSettings, string outFile) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutFile = outFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixEncryptSettings.OutFile"/></em></p>
    ///   <p>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</p>
    /// </summary>
    [Pure]
    public static T ResetOutFile<T>(this T toolSettings) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutFile = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixEncryptSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixEncryptSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixEncryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixDecryptSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixDecryptSettingsExtensions
{
    #region Environment
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixDecryptSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T SetEnvironment<T>(this T toolSettings, string environment) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = environment;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixDecryptSettings.Environment"/></em></p>
    ///   <p>The name of the environment to run the command in. Overrules the active environment set in .confixrc</p>
    /// </summary>
    [Pure]
    public static T ResetEnvironment<T>(this T toolSettings) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Environment = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixDecryptSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixDecryptSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region InputFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixDecryptSettings.InputFile"/></em></p>
    ///   <p>The path to the file to encrypt or decrypt.</p>
    /// </summary>
    [Pure]
    public static T SetInputFile<T>(this T toolSettings, string inputFile) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.InputFile = inputFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixDecryptSettings.InputFile"/></em></p>
    ///   <p>The path to the file to encrypt or decrypt.</p>
    /// </summary>
    [Pure]
    public static T ResetInputFile<T>(this T toolSettings) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.InputFile = null;
        return toolSettings;
    }
    #endregion
    #region OutFile
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixDecryptSettings.OutFile"/></em></p>
    ///   <p>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</p>
    /// </summary>
    [Pure]
    public static T SetOutFile<T>(this T toolSettings, string outFile) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutFile = outFile;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixDecryptSettings.OutFile"/></em></p>
    ///   <p>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</p>
    /// </summary>
    [Pure]
    public static T ResetOutFile<T>(this T toolSettings) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutFile = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixDecryptSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixDecryptSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixDecryptSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixConfigShowSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigShowSettingsExtensions
{
    #region Format
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigShowSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T SetFormat<T>(this T toolSettings, string format) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = format;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigShowSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T ResetFormat<T>(this T toolSettings) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigShowSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigShowSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigShowSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigShowSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixConfigShowSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixConfigSetSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigSetSettingsExtensions
{
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigSetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigSetSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Path
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigSetSettings.Path"/></em></p>
    ///   <p>The path to the configuration file</p>
    /// </summary>
    [Pure]
    public static T SetPath<T>(this T toolSettings, string path) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Path = path;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigSetSettings.Path"/></em></p>
    ///   <p>The path to the configuration file</p>
    /// </summary>
    [Pure]
    public static T ResetPath<T>(this T toolSettings) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Path = null;
        return toolSettings;
    }
    #endregion
    #region Value
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigSetSettings.Value"/></em></p>
    ///   <p>The value to set as json</p>
    /// </summary>
    [Pure]
    public static T SetValue<T>(this T toolSettings, string value) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Value = value;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigSetSettings.Value"/></em></p>
    ///   <p>The value to set as json</p>
    /// </summary>
    [Pure]
    public static T ResetValue<T>(this T toolSettings) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Value = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigSetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigSetSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixConfigSetSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region ConfixConfigListSettingsExtensions
/// <summary>
///   Used within <see cref="ConfixTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigListSettingsExtensions
{
    #region Format
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigListSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T SetFormat<T>(this T toolSettings, string format) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = format;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigListSettings.Format"/></em></p>
    ///   <p>Sets the output format</p>
    /// </summary>
    [Pure]
    public static T ResetFormat<T>(this T toolSettings) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = null;
        return toolSettings;
    }
    #endregion
    #region Verbosity
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T SetVerbosity<T>(this T toolSettings, string verbosity) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = verbosity;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigListSettings.Verbosity"/></em></p>
    ///   <p>Sets the verbosity level</p>
    /// </summary>
    [Pure]
    public static T ResetVerbosity<T>(this T toolSettings) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Verbosity = null;
        return toolSettings;
    }
    #endregion
    #region Framework
    /// <summary>
    ///   <p><em>Sets <see cref="ConfixConfigListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFramework<T>(this T toolSettings, string framework) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = framework;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="ConfixConfigListSettings.Framework"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFramework<T>(this T toolSettings) where T : ConfixConfigListSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Framework = null;
        return toolSettings;
    }
    #endregion
}
#endregion
