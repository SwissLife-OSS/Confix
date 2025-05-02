
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

/// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public partial class ConfixTasks : ToolTasks
{
    public static string ConfixPath => new ConfixTasks().GetToolPath();
    public const string PackageExecutable = "Confix.dll";
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    public static IReadOnlyCollection<Output> Confix(ArgumentStringHandler arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Action<OutputType, string> logger = null, Func<IProcess, object> exitHandler = null) => new ConfixTasks().Run(arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, logger, exitHandler);
    /// <summary><p>builds a component. Runs all configured component inputs</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentBuild(ConfixComponentBuildSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>builds a component. Runs all configured component inputs</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentBuild(Configure<ConfixComponentBuildSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixComponentBuildSettings()));
    /// <summary><p>builds a component. Runs all configured component inputs</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixComponentBuildSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixComponentBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentBuild(CombinatorialConfigure<ConfixComponentBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixComponentBuild, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Initializes a component and creates a component file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentInit(ConfixComponentInitSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Initializes a component and creates a component file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentInit(Configure<ConfixComponentInitSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixComponentInitSettings()));
    /// <summary><p>Initializes a component and creates a component file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentInitSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentInitSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixComponentInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentInit(CombinatorialConfigure<ConfixComponentInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixComponentInit, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Lists the component of the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li><li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentList(ConfixComponentListSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Lists the component of the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li><li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentList(Configure<ConfixComponentListSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixComponentListSettings()));
    /// <summary><p>Lists the component of the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixComponentListSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixComponentListSettings.Format"/></li><li><c>--only-components</c> via <see cref="ConfixComponentListSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixComponentListSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentListSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixComponentListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentList(CombinatorialConfigure<ConfixComponentListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixComponentList, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Adds a component to the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li><li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li><li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentAdd(ConfixComponentAddSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Adds a component to the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li><li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li><li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixComponentAdd(Configure<ConfixComponentAddSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixComponentAddSettings()));
    /// <summary><p>Adds a component to the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;name&gt;</c> via <see cref="ConfixComponentAddSettings.Name"/></li><li><c>--output-file</c> via <see cref="ConfixComponentAddSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixComponentAddSettings.Verbosity"/></li><li><c>--version</c> via <see cref="ConfixComponentAddSettings.Version"/></li></ul></remarks>
    public static IEnumerable<(ConfixComponentAddSettings Settings, IReadOnlyCollection<Output> Output)> ConfixComponentAdd(CombinatorialConfigure<ConfixComponentAddSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixComponentAdd, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Reloads the schema of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectRestore(ConfixProjectRestoreSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Reloads the schema of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectRestore(Configure<ConfixProjectRestoreSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixProjectRestoreSettings()));
    /// <summary><p>Reloads the schema of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectRestoreSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectRestoreSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectRestoreSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixProjectRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectRestore(CombinatorialConfigure<ConfixProjectRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixProjectRestore, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Replaces all variables in the project files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectBuild(ConfixProjectBuildSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Replaces all variables in the project files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectBuild(Configure<ConfixProjectBuildSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixProjectBuildSettings()));
    /// <summary><p>Replaces all variables in the project files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixProjectBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixProjectBuildSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectBuildSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectBuildSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectBuildSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixProjectBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectBuild(CombinatorialConfigure<ConfixProjectBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixProjectBuild, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Initializes a project and creates a project file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectInit(ConfixProjectInitSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Initializes a project and creates a project file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectInit(Configure<ConfixProjectInitSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixProjectInitSettings()));
    /// <summary><p>Initializes a project and creates a project file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--output-file</c> via <see cref="ConfixProjectInitSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectInitSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixProjectInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectInit(CombinatorialConfigure<ConfixProjectInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixProjectInit, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Validates the configuration files of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectValidate(ConfixProjectValidateSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Validates the configuration files of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectValidate(Configure<ConfixProjectValidateSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixProjectValidateSettings()));
    /// <summary><p>Validates the configuration files of a project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectValidateSettings.Environment"/></li><li><c>--only-components</c> via <see cref="ConfixProjectValidateSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectValidateSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectValidateSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixProjectValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectValidate(CombinatorialConfigure<ConfixProjectValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixProjectValidate, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Generates a report for the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectReport(ConfixProjectReportSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Generates a report for the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixProjectReport(Configure<ConfixProjectReportSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixProjectReportSettings()));
    /// <summary><p>Generates a report for the project</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixProjectReportSettings.Environment"/></li><li><c>--no-restore</c> via <see cref="ConfixProjectReportSettings.NoRestore"/></li><li><c>--only-components</c> via <see cref="ConfixProjectReportSettings.OnlyComponents"/></li><li><c>--output-file</c> via <see cref="ConfixProjectReportSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixProjectReportSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixProjectReportSettings Settings, IReadOnlyCollection<Output> Output)> ConfixProjectReport(CombinatorialConfigure<ConfixProjectReportSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixProjectReport, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Reloads the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionRestore(ConfixSolutionRestoreSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Reloads the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionRestore(Configure<ConfixSolutionRestoreSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixSolutionRestoreSettings()));
    /// <summary><p>Reloads the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixSolutionRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionRestore(CombinatorialConfigure<ConfixSolutionRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixSolutionRestore, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Replaces all variables in the solution files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionBuild(ConfixSolutionBuildSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Replaces all variables in the solution files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionBuild(Configure<ConfixSolutionBuildSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixSolutionBuildSettings()));
    /// <summary><p>Replaces all variables in the solution files with their values</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionBuildSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixSolutionBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionBuild(CombinatorialConfigure<ConfixSolutionBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixSolutionBuild, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Initializes a solution and creates a solution file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionInit(ConfixSolutionInitSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Initializes a solution and creates a solution file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionInit(Configure<ConfixSolutionInitSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixSolutionInitSettings()));
    /// <summary><p>Initializes a solution and creates a solution file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionInitSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixSolutionInitSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionInit(CombinatorialConfigure<ConfixSolutionInitSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixSolutionInit, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Validates the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionValidate(ConfixSolutionValidateSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Validates the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixSolutionValidate(Configure<ConfixSolutionValidateSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixSolutionValidateSettings()));
    /// <summary><p>Validates the schema of all the projects in the solution</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--verbosity</c> via <see cref="ConfixSolutionValidateSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixSolutionValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixSolutionValidate(CombinatorialConfigure<ConfixSolutionValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixSolutionValidate, degreeOfParallelism, completeOnFailure);
    /// <summary><p>resolves a variable by name</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li><li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableGet(ConfixVariableGetSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>resolves a variable by name</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li><li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableGet(Configure<ConfixVariableGetSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixVariableGetSettings()));
    /// <summary><p>resolves a variable by name</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableGetSettings.Environment"/></li><li><c>--format</c> via <see cref="ConfixVariableGetSettings.Format"/></li><li><c>--name</c> via <see cref="ConfixVariableGetSettings.Name"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableGetSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixVariableGetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableGet(CombinatorialConfigure<ConfixVariableGetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixVariableGet, degreeOfParallelism, completeOnFailure);
    /// <summary><p>sets a variable. Overrides existing value if any.</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li><li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li><li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableSet(ConfixVariableSetSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>sets a variable. Overrides existing value if any.</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li><li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li><li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableSet(Configure<ConfixVariableSetSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixVariableSetSettings()));
    /// <summary><p>sets a variable. Overrides existing value if any.</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableSetSettings.Environment"/></li><li><c>--name</c> via <see cref="ConfixVariableSetSettings.Name"/></li><li><c>--value</c> via <see cref="ConfixVariableSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableSetSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixVariableSetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableSet(CombinatorialConfigure<ConfixVariableSetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixVariableSet, degreeOfParallelism, completeOnFailure);
    /// <summary><p>list available variables</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li><li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableList(ConfixVariableListSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>list available variables</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li><li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableList(Configure<ConfixVariableListSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixVariableListSettings()));
    /// <summary><p>list available variables</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableListSettings.Environment"/></li><li><c>--provider</c> via <see cref="ConfixVariableListSettings.Provider"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableListSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixVariableListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableList(CombinatorialConfigure<ConfixVariableListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixVariableList, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Copies a variable from one provider to another provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li><li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li><li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li><li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableCopy(ConfixVariableCopySettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Copies a variable from one provider to another provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li><li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li><li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li><li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixVariableCopy(Configure<ConfixVariableCopySettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixVariableCopySettings()));
    /// <summary><p>Copies a variable from one provider to another provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixVariableCopySettings.Environment"/></li><li><c>--from</c> via <see cref="ConfixVariableCopySettings.From"/></li><li><c>--to</c> via <see cref="ConfixVariableCopySettings.To"/></li><li><c>--to-environment</c> via <see cref="ConfixVariableCopySettings.ToEnvironment"/></li><li><c>--verbosity</c> via <see cref="ConfixVariableCopySettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixVariableCopySettings Settings, IReadOnlyCollection<Output> Output)> ConfixVariableCopy(CombinatorialConfigure<ConfixVariableCopySettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixVariableCopy, degreeOfParallelism, completeOnFailure);
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li><li><c>--git-token</c> via <see cref="ConfixBuildSettings.GitToken"/></li><li><c>--git-username</c> via <see cref="ConfixBuildSettings.GitUsername"/></li><li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixBuild(ConfixBuildSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li><li><c>--git-token</c> via <see cref="ConfixBuildSettings.GitToken"/></li><li><c>--git-username</c> via <see cref="ConfixBuildSettings.GitUsername"/></li><li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixBuild(Configure<ConfixBuildSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixBuildSettings()));
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--encrypt</c> via <see cref="ConfixBuildSettings.Encrypt"/></li><li><c>--environment</c> via <see cref="ConfixBuildSettings.Environment"/></li><li><c>--git-token</c> via <see cref="ConfixBuildSettings.GitToken"/></li><li><c>--git-username</c> via <see cref="ConfixBuildSettings.GitUsername"/></li><li><c>--output-file</c> via <see cref="ConfixBuildSettings.OutputFile"/></li><li><c>--verbosity</c> via <see cref="ConfixBuildSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixBuildSettings Settings, IReadOnlyCollection<Output> Output)> ConfixBuild(CombinatorialConfigure<ConfixBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixBuild, degreeOfParallelism, completeOnFailure);
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--dotnet-configuration</c> via <see cref="ConfixRestoreSettings.DotnetConfiguration"/></li><li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixRestore(ConfixRestoreSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--dotnet-configuration</c> via <see cref="ConfixRestoreSettings.DotnetConfiguration"/></li><li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixRestore(Configure<ConfixRestoreSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixRestoreSettings()));
    /// <summary><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--dotnet-configuration</c> via <see cref="ConfixRestoreSettings.DotnetConfiguration"/></li><li><c>--environment</c> via <see cref="ConfixRestoreSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixRestoreSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixRestoreSettings Settings, IReadOnlyCollection<Output> Output)> ConfixRestore(CombinatorialConfigure<ConfixRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixRestore, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Validates the schema of all the projects</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixValidate(ConfixValidateSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Validates the schema of all the projects</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixValidate(Configure<ConfixValidateSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixValidateSettings()));
    /// <summary><p>Validates the schema of all the projects</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--environment</c> via <see cref="ConfixValidateSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixValidateSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixValidateSettings Settings, IReadOnlyCollection<Output> Output)> ConfixValidate(CombinatorialConfigure<ConfixValidateSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixValidate, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Encrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixEncrypt(ConfixEncryptSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Encrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixEncrypt(Configure<ConfixEncryptSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixEncryptSettings()));
    /// <summary><p>Encrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixEncryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixEncryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixEncryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixEncryptSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixEncryptSettings Settings, IReadOnlyCollection<Output> Output)> ConfixEncrypt(CombinatorialConfigure<ConfixEncryptSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixEncrypt, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Decrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixDecrypt(ConfixDecryptSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Decrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixDecrypt(Configure<ConfixDecryptSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixDecryptSettings()));
    /// <summary><p>Decrypts a file using the configured provider</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;inputFile&gt;</c> via <see cref="ConfixDecryptSettings.InputFile"/></li><li><c>&lt;outFile&gt;</c> via <see cref="ConfixDecryptSettings.OutFile"/></li><li><c>--environment</c> via <see cref="ConfixDecryptSettings.Environment"/></li><li><c>--verbosity</c> via <see cref="ConfixDecryptSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixDecryptSettings Settings, IReadOnlyCollection<Output> Output)> ConfixDecrypt(CombinatorialConfigure<ConfixDecryptSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixDecrypt, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Shows the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigShow(ConfixConfigShowSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Shows the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigShow(Configure<ConfixConfigShowSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixConfigShowSettings()));
    /// <summary><p>Shows the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigShowSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigShowSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixConfigShowSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigShow(CombinatorialConfigure<ConfixConfigShowSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixConfigShow, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Sets a configuration value in the nearest .confixrc</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li><li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigSet(ConfixConfigSetSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Sets a configuration value in the nearest .confixrc</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li><li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigSet(Configure<ConfixConfigSetSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixConfigSetSettings()));
    /// <summary><p>Sets a configuration value in the nearest .confixrc</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>&lt;path&gt;</c> via <see cref="ConfixConfigSetSettings.Path"/></li><li><c>&lt;value&gt;</c> via <see cref="ConfixConfigSetSettings.Value"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigSetSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixConfigSetSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigSet(CombinatorialConfigure<ConfixConfigSetSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixConfigSet, degreeOfParallelism, completeOnFailure);
    /// <summary><p>Lists the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigList(ConfixConfigListSettings options = null) => new ConfixTasks().Run(options);
    /// <summary><p>Lists the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li></ul></remarks>
    public static IReadOnlyCollection<Output> ConfixConfigList(Configure<ConfixConfigListSettings> configurator) => new ConfixTasks().Run(configurator.Invoke(new ConfixConfigListSettings()));
    /// <summary><p>Lists the configuration to a file</p><p>For more details, visit the <a href="https://swisslife-oss.github.io/Confix/">official website</a>.</p></summary>
    /// <remarks><p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p><ul><li><c>--format</c> via <see cref="ConfixConfigListSettings.Format"/></li><li><c>--verbosity</c> via <see cref="ConfixConfigListSettings.Verbosity"/></li></ul></remarks>
    public static IEnumerable<(ConfixConfigListSettings Settings, IReadOnlyCollection<Output> Output)> ConfixConfigList(CombinatorialConfigure<ConfixConfigListSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false) => configurator.Invoke(ConfixConfigList, degreeOfParallelism, completeOnFailure);
}
#region ConfixComponentBuildSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixComponentBuild), Arguments = "component build")]
public partial class ConfixComponentBuildSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixComponentInitSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixComponentInit), Arguments = "component init")]
public partial class ConfixComponentInitSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary>The name of the component</summary>
    [Argument(Format = "{value}")] public string Name => Get<string>(() => Name);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixComponentListSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixComponentList), Arguments = "component list")]
public partial class ConfixComponentListSettings : ToolOptions
{
    /// <summary>Sets the output format</summary>
    [Argument(Format = "--format {value}")] public string Format => Get<string>(() => Format);
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>If you specify this option, only the components will be built.</summary>
    [Argument(Format = "--only-components {value}")] public string OnlyComponents => Get<string>(() => OnlyComponents);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixComponentAddSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixComponentAdd), Arguments = "component add")]
public partial class ConfixComponentAddSettings : ToolOptions
{
    /// <summary>Shows the version information</summary>
    [Argument(Format = "--version {value}")] public string Version => Get<string>(() => Version);
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary>The name of the component</summary>
    [Argument(Format = "{value}")] public string Name => Get<string>(() => Name);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixProjectRestoreSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixProjectRestore), Arguments = "project restore")]
public partial class ConfixProjectRestoreSettings : ToolOptions
{
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>If you specify this option, only the components will be built.</summary>
    [Argument(Format = "--only-components {value}")] public string OnlyComponents => Get<string>(() => OnlyComponents);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixProjectBuildSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixProjectBuild), Arguments = "project build")]
public partial class ConfixProjectBuildSettings : ToolOptions
{
    /// <summary>Disables restoring of schemas</summary>
    [Argument(Format = "--no-restore {value}")] public string NoRestore => Get<string>(() => NoRestore);
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>If you specify this option, only the components will be built.</summary>
    [Argument(Format = "--only-components {value}")] public string OnlyComponents => Get<string>(() => OnlyComponents);
    /// <summary>Encrypt the output file</summary>
    [Argument(Format = "--encrypt {value}")] public string Encrypt => Get<string>(() => Encrypt);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixProjectInitSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixProjectInit), Arguments = "project init")]
public partial class ConfixProjectInitSettings : ToolOptions
{
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixProjectValidateSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixProjectValidate), Arguments = "project validate")]
public partial class ConfixProjectValidateSettings : ToolOptions
{
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>If you specify this option, only the components will be built.</summary>
    [Argument(Format = "--only-components {value}")] public string OnlyComponents => Get<string>(() => OnlyComponents);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixProjectReportSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixProjectReport), Arguments = "project report")]
public partial class ConfixProjectReportSettings : ToolOptions
{
    /// <summary>Disables restoring of schemas</summary>
    [Argument(Format = "--no-restore {value}")] public string NoRestore => Get<string>(() => NoRestore);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>The path to the report file. If not specified, the report will be written to the console.</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>If you specify this option, only the components will be built.</summary>
    [Argument(Format = "--only-components {value}")] public string OnlyComponents => Get<string>(() => OnlyComponents);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixSolutionRestoreSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixSolutionRestore), Arguments = "solution restore")]
public partial class ConfixSolutionRestoreSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixSolutionBuildSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixSolutionBuild), Arguments = "solution build")]
public partial class ConfixSolutionBuildSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixSolutionInitSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixSolutionInit), Arguments = "solution init")]
public partial class ConfixSolutionInitSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixSolutionValidateSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixSolutionValidate), Arguments = "solution validate")]
public partial class ConfixSolutionValidateSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixVariableGetSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixVariableGet), Arguments = "variable get")]
public partial class ConfixVariableGetSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>The name of the variable</summary>
    [Argument(Format = "--name {value}")] public string Name => Get<string>(() => Name);
    /// <summary>Sets the output format</summary>
    [Argument(Format = "--format {value}")] public string Format => Get<string>(() => Format);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixVariableSetSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixVariableSet), Arguments = "variable set")]
public partial class ConfixVariableSetSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>The name of the variable</summary>
    [Argument(Format = "--name {value}")] public string Name => Get<string>(() => Name);
    /// <summary>The value of the variable</summary>
    [Argument(Format = "--value {value}")] public string Value => Get<string>(() => Value);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixVariableListSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixVariableList), Arguments = "variable list")]
public partial class ConfixVariableListSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>The name of the provider to resolve the variable from</summary>
    [Argument(Format = "--provider {value}")] public string Provider => Get<string>(() => Provider);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixVariableCopySettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixVariableCopy), Arguments = "variable copy")]
public partial class ConfixVariableCopySettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>The name of the new variable</summary>
    [Argument(Format = "--from {value}")] public string From => Get<string>(() => From);
    /// <summary>The name of the new variable</summary>
    [Argument(Format = "--to {value}")] public string To => Get<string>(() => To);
    /// <summary>The name of the environment you want to migrate the variable to</summary>
    [Argument(Format = "--to-environment {value}")] public string ToEnvironment => Get<string>(() => ToEnvironment);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixBuildSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixBuild), Arguments = "build")]
public partial class ConfixBuildSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>Specifies the output file</summary>
    [Argument(Format = "--output-file {value}")] public string OutputFile => Get<string>(() => OutputFile);
    /// <summary>Encrypt the output file</summary>
    [Argument(Format = "--encrypt {value}")] public string Encrypt => Get<string>(() => Encrypt);
    /// <summary>The username used for git authentication.</summary>
    [Argument(Format = "--git-username {value}")] public string GitUsername => Get<string>(() => GitUsername);
    /// <summary>The token used for git authentication.</summary>
    [Argument(Format = "--git-token {value}")] public string GitToken => Get<string>(() => GitToken);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixRestoreSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixRestore), Arguments = "restore")]
public partial class ConfixRestoreSettings : ToolOptions
{
    /// <summary>The configuration passed to dotnet commands. Defaults to 'Debug'.</summary>
    [Argument(Format = "--dotnet-configuration {value}")] public string DotnetConfiguration => Get<string>(() => DotnetConfiguration);
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixValidateSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixValidate), Arguments = "validate")]
public partial class ConfixValidateSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixEncryptSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixEncrypt), Arguments = "encrypt")]
public partial class ConfixEncryptSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary>The path to the file to encrypt or decrypt.</summary>
    [Argument(Format = "{value}")] public string InputFile => Get<string>(() => InputFile);
    /// <summary>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</summary>
    [Argument(Format = "{value}")] public string OutFile => Get<string>(() => OutFile);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixDecryptSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixDecrypt), Arguments = "decrypt")]
public partial class ConfixDecryptSettings : ToolOptions
{
    /// <summary>The name of the environment to run the command in. Overrules the active environment set in .confixrc</summary>
    [Argument(Format = "--environment {value}")] public string Environment => Get<string>(() => Environment);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary>The path to the file to encrypt or decrypt.</summary>
    [Argument(Format = "{value}")] public string InputFile => Get<string>(() => InputFile);
    /// <summary>The file to write the encrypted or decrypted data to.  If not provided the input file will be overwritten.  Existing files will be overwritten.</summary>
    [Argument(Format = "{value}")] public string OutFile => Get<string>(() => OutFile);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixConfigShowSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixConfigShow), Arguments = "config show")]
public partial class ConfixConfigShowSettings : ToolOptions
{
    /// <summary>Sets the output format</summary>
    [Argument(Format = "--format {value}")] public string Format => Get<string>(() => Format);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixConfigSetSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixConfigSet), Arguments = "config set")]
public partial class ConfixConfigSetSettings : ToolOptions
{
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary>The path to the configuration file</summary>
    [Argument(Format = "{value}")] public string Path => Get<string>(() => Path);
    /// <summary>The value to set as json</summary>
    [Argument(Format = "{value}")] public string Value => Get<string>(() => Value);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixConfigListSettings
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Command(Type = typeof(ConfixTasks), Command = nameof(ConfixTasks.ConfixConfigList), Arguments = "config list")]
public partial class ConfixConfigListSettings : ToolOptions
{
    /// <summary>Sets the output format</summary>
    [Argument(Format = "--format {value}")] public string Format => Get<string>(() => Format);
    /// <summary>Sets the verbosity level</summary>
    [Argument(Format = "--verbosity {value}")] public string Verbosity => Get<string>(() => Verbosity);
    /// <summary></summary>
    public string Framework => Get<string>(() => Framework);
}
#endregion
#region ConfixComponentBuildSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentBuildSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixComponentBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentBuildSettings), Property = nameof(ConfixComponentBuildSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixComponentBuildSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixComponentBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentBuildSettings), Property = nameof(ConfixComponentBuildSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixComponentBuildSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixComponentBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentBuildSettings), Property = nameof(ConfixComponentBuildSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixComponentBuildSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixComponentBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentBuildSettings), Property = nameof(ConfixComponentBuildSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixComponentBuildSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixComponentInitSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentInitSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixComponentInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixComponentInitSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixComponentInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixComponentInitSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Name
    /// <inheritdoc cref="ConfixComponentInitSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Name))]
    public static T SetName<T>(this T o, string v) where T : ConfixComponentInitSettings => o.Modify(b => b.Set(() => o.Name, v));
    /// <inheritdoc cref="ConfixComponentInitSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Name))]
    public static T ResetName<T>(this T o) where T : ConfixComponentInitSettings => o.Modify(b => b.Remove(() => o.Name));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixComponentInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixComponentInitSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixComponentInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentInitSettings), Property = nameof(ConfixComponentInitSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixComponentInitSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixComponentListSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentListSettingsExtensions
{
    #region Format
    /// <inheritdoc cref="ConfixComponentListSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Format))]
    public static T SetFormat<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.Format, v));
    /// <inheritdoc cref="ConfixComponentListSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Format))]
    public static T ResetFormat<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.Format));
    #endregion
    #region OutputFile
    /// <inheritdoc cref="ConfixComponentListSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixComponentListSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixComponentListSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixComponentListSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OnlyComponents
    /// <inheritdoc cref="ConfixComponentListSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.OnlyComponents))]
    public static T SetOnlyComponents<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.OnlyComponents, v));
    /// <inheritdoc cref="ConfixComponentListSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.OnlyComponents))]
    public static T ResetOnlyComponents<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.OnlyComponents));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixComponentListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixComponentListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixComponentListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixComponentListSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixComponentListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentListSettings), Property = nameof(ConfixComponentListSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixComponentListSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixComponentAddSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixComponentAddSettingsExtensions
{
    #region Version
    /// <inheritdoc cref="ConfixComponentAddSettings.Version"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Version))]
    public static T SetVersion<T>(this T o, string v) where T : ConfixComponentAddSettings => o.Modify(b => b.Set(() => o.Version, v));
    /// <inheritdoc cref="ConfixComponentAddSettings.Version"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Version))]
    public static T ResetVersion<T>(this T o) where T : ConfixComponentAddSettings => o.Modify(b => b.Remove(() => o.Version));
    #endregion
    #region OutputFile
    /// <inheritdoc cref="ConfixComponentAddSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixComponentAddSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixComponentAddSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixComponentAddSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixComponentAddSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixComponentAddSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixComponentAddSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixComponentAddSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Name
    /// <inheritdoc cref="ConfixComponentAddSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Name))]
    public static T SetName<T>(this T o, string v) where T : ConfixComponentAddSettings => o.Modify(b => b.Set(() => o.Name, v));
    /// <inheritdoc cref="ConfixComponentAddSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Name))]
    public static T ResetName<T>(this T o) where T : ConfixComponentAddSettings => o.Modify(b => b.Remove(() => o.Name));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixComponentAddSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixComponentAddSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixComponentAddSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixComponentAddSettings), Property = nameof(ConfixComponentAddSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixComponentAddSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixProjectRestoreSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectRestoreSettingsExtensions
{
    #region OutputFile
    /// <inheritdoc cref="ConfixProjectRestoreSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixProjectRestoreSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OnlyComponents
    /// <inheritdoc cref="ConfixProjectRestoreSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.OnlyComponents))]
    public static T SetOnlyComponents<T>(this T o, string v) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Set(() => o.OnlyComponents, v));
    /// <inheritdoc cref="ConfixProjectRestoreSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.OnlyComponents))]
    public static T ResetOnlyComponents<T>(this T o) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Remove(() => o.OnlyComponents));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixProjectRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectRestoreSettings), Property = nameof(ConfixProjectRestoreSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixProjectRestoreSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixProjectBuildSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectBuildSettingsExtensions
{
    #region NoRestore
    /// <inheritdoc cref="ConfixProjectBuildSettings.NoRestore"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.NoRestore))]
    public static T SetNoRestore<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.NoRestore, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.NoRestore"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.NoRestore))]
    public static T ResetNoRestore<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.NoRestore));
    #endregion
    #region OutputFile
    /// <inheritdoc cref="ConfixProjectBuildSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixProjectBuildSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OnlyComponents
    /// <inheritdoc cref="ConfixProjectBuildSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.OnlyComponents))]
    public static T SetOnlyComponents<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.OnlyComponents, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.OnlyComponents))]
    public static T ResetOnlyComponents<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.OnlyComponents));
    #endregion
    #region Encrypt
    /// <inheritdoc cref="ConfixProjectBuildSettings.Encrypt"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Encrypt))]
    public static T SetEncrypt<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.Encrypt, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.Encrypt"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Encrypt))]
    public static T ResetEncrypt<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.Encrypt));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixProjectBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixProjectBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixProjectBuildSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixProjectBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectBuildSettings), Property = nameof(ConfixProjectBuildSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixProjectBuildSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixProjectInitSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectInitSettingsExtensions
{
    #region OutputFile
    /// <inheritdoc cref="ConfixProjectInitSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixProjectInitSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixProjectInitSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixProjectInitSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixProjectInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixProjectInitSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixProjectInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixProjectInitSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixProjectInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixProjectInitSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixProjectInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectInitSettings), Property = nameof(ConfixProjectInitSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixProjectInitSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixProjectValidateSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectValidateSettingsExtensions
{
    #region OutputFile
    /// <inheritdoc cref="ConfixProjectValidateSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixProjectValidateSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixProjectValidateSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixProjectValidateSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixProjectValidateSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixProjectValidateSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixProjectValidateSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixProjectValidateSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OnlyComponents
    /// <inheritdoc cref="ConfixProjectValidateSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.OnlyComponents))]
    public static T SetOnlyComponents<T>(this T o, string v) where T : ConfixProjectValidateSettings => o.Modify(b => b.Set(() => o.OnlyComponents, v));
    /// <inheritdoc cref="ConfixProjectValidateSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.OnlyComponents))]
    public static T ResetOnlyComponents<T>(this T o) where T : ConfixProjectValidateSettings => o.Modify(b => b.Remove(() => o.OnlyComponents));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixProjectValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixProjectValidateSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixProjectValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixProjectValidateSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixProjectValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixProjectValidateSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixProjectValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectValidateSettings), Property = nameof(ConfixProjectValidateSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixProjectValidateSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixProjectReportSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixProjectReportSettingsExtensions
{
    #region NoRestore
    /// <inheritdoc cref="ConfixProjectReportSettings.NoRestore"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.NoRestore))]
    public static T SetNoRestore<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.NoRestore, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.NoRestore"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.NoRestore))]
    public static T ResetNoRestore<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.NoRestore));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixProjectReportSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OutputFile
    /// <inheritdoc cref="ConfixProjectReportSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region OnlyComponents
    /// <inheritdoc cref="ConfixProjectReportSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.OnlyComponents))]
    public static T SetOnlyComponents<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.OnlyComponents, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.OnlyComponents"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.OnlyComponents))]
    public static T ResetOnlyComponents<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.OnlyComponents));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixProjectReportSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixProjectReportSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixProjectReportSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixProjectReportSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixProjectReportSettings), Property = nameof(ConfixProjectReportSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixProjectReportSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixSolutionRestoreSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionRestoreSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixSolutionRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionRestoreSettings), Property = nameof(ConfixSolutionRestoreSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixSolutionRestoreSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixSolutionRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionRestoreSettings), Property = nameof(ConfixSolutionRestoreSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixSolutionRestoreSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixSolutionRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionRestoreSettings), Property = nameof(ConfixSolutionRestoreSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixSolutionRestoreSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixSolutionRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionRestoreSettings), Property = nameof(ConfixSolutionRestoreSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixSolutionRestoreSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixSolutionBuildSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionBuildSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixSolutionBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionBuildSettings), Property = nameof(ConfixSolutionBuildSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixSolutionBuildSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixSolutionBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionBuildSettings), Property = nameof(ConfixSolutionBuildSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixSolutionBuildSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixSolutionBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionBuildSettings), Property = nameof(ConfixSolutionBuildSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixSolutionBuildSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixSolutionBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionBuildSettings), Property = nameof(ConfixSolutionBuildSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixSolutionBuildSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixSolutionInitSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionInitSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixSolutionInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionInitSettings), Property = nameof(ConfixSolutionInitSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixSolutionInitSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixSolutionInitSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionInitSettings), Property = nameof(ConfixSolutionInitSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixSolutionInitSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixSolutionInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionInitSettings), Property = nameof(ConfixSolutionInitSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixSolutionInitSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixSolutionInitSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionInitSettings), Property = nameof(ConfixSolutionInitSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixSolutionInitSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixSolutionValidateSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixSolutionValidateSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixSolutionValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionValidateSettings), Property = nameof(ConfixSolutionValidateSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixSolutionValidateSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixSolutionValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionValidateSettings), Property = nameof(ConfixSolutionValidateSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixSolutionValidateSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixSolutionValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionValidateSettings), Property = nameof(ConfixSolutionValidateSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixSolutionValidateSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixSolutionValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixSolutionValidateSettings), Property = nameof(ConfixSolutionValidateSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixSolutionValidateSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixVariableGetSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableGetSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixVariableGetSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixVariableGetSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixVariableGetSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixVariableGetSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Name
    /// <inheritdoc cref="ConfixVariableGetSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Name))]
    public static T SetName<T>(this T o, string v) where T : ConfixVariableGetSettings => o.Modify(b => b.Set(() => o.Name, v));
    /// <inheritdoc cref="ConfixVariableGetSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Name))]
    public static T ResetName<T>(this T o) where T : ConfixVariableGetSettings => o.Modify(b => b.Remove(() => o.Name));
    #endregion
    #region Format
    /// <inheritdoc cref="ConfixVariableGetSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Format))]
    public static T SetFormat<T>(this T o, string v) where T : ConfixVariableGetSettings => o.Modify(b => b.Set(() => o.Format, v));
    /// <inheritdoc cref="ConfixVariableGetSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Format))]
    public static T ResetFormat<T>(this T o) where T : ConfixVariableGetSettings => o.Modify(b => b.Remove(() => o.Format));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixVariableGetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixVariableGetSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixVariableGetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixVariableGetSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixVariableGetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixVariableGetSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixVariableGetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableGetSettings), Property = nameof(ConfixVariableGetSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixVariableGetSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixVariableSetSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableSetSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixVariableSetSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixVariableSetSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixVariableSetSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixVariableSetSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Name
    /// <inheritdoc cref="ConfixVariableSetSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Name))]
    public static T SetName<T>(this T o, string v) where T : ConfixVariableSetSettings => o.Modify(b => b.Set(() => o.Name, v));
    /// <inheritdoc cref="ConfixVariableSetSettings.Name"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Name))]
    public static T ResetName<T>(this T o) where T : ConfixVariableSetSettings => o.Modify(b => b.Remove(() => o.Name));
    #endregion
    #region Value
    /// <inheritdoc cref="ConfixVariableSetSettings.Value"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Value))]
    public static T SetValue<T>(this T o, string v) where T : ConfixVariableSetSettings => o.Modify(b => b.Set(() => o.Value, v));
    /// <inheritdoc cref="ConfixVariableSetSettings.Value"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Value))]
    public static T ResetValue<T>(this T o) where T : ConfixVariableSetSettings => o.Modify(b => b.Remove(() => o.Value));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixVariableSetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixVariableSetSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixVariableSetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixVariableSetSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixVariableSetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixVariableSetSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixVariableSetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableSetSettings), Property = nameof(ConfixVariableSetSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixVariableSetSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixVariableListSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableListSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixVariableListSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixVariableListSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixVariableListSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixVariableListSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Provider
    /// <inheritdoc cref="ConfixVariableListSettings.Provider"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Provider))]
    public static T SetProvider<T>(this T o, string v) where T : ConfixVariableListSettings => o.Modify(b => b.Set(() => o.Provider, v));
    /// <inheritdoc cref="ConfixVariableListSettings.Provider"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Provider))]
    public static T ResetProvider<T>(this T o) where T : ConfixVariableListSettings => o.Modify(b => b.Remove(() => o.Provider));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixVariableListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixVariableListSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixVariableListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixVariableListSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixVariableListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixVariableListSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixVariableListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableListSettings), Property = nameof(ConfixVariableListSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixVariableListSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixVariableCopySettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixVariableCopySettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixVariableCopySettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region From
    /// <inheritdoc cref="ConfixVariableCopySettings.From"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.From))]
    public static T SetFrom<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.From, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.From"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.From))]
    public static T ResetFrom<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.From));
    #endregion
    #region To
    /// <inheritdoc cref="ConfixVariableCopySettings.To"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.To))]
    public static T SetTo<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.To, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.To"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.To))]
    public static T ResetTo<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.To));
    #endregion
    #region ToEnvironment
    /// <inheritdoc cref="ConfixVariableCopySettings.ToEnvironment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.ToEnvironment))]
    public static T SetToEnvironment<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.ToEnvironment, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.ToEnvironment"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.ToEnvironment))]
    public static T ResetToEnvironment<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.ToEnvironment));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixVariableCopySettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixVariableCopySettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixVariableCopySettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixVariableCopySettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixVariableCopySettings), Property = nameof(ConfixVariableCopySettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixVariableCopySettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixBuildSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixBuildSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixBuildSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixBuildSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region OutputFile
    /// <inheritdoc cref="ConfixBuildSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.OutputFile))]
    public static T SetOutputFile<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.OutputFile, v));
    /// <inheritdoc cref="ConfixBuildSettings.OutputFile"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.OutputFile))]
    public static T ResetOutputFile<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.OutputFile));
    #endregion
    #region Encrypt
    /// <inheritdoc cref="ConfixBuildSettings.Encrypt"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Encrypt))]
    public static T SetEncrypt<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.Encrypt, v));
    /// <inheritdoc cref="ConfixBuildSettings.Encrypt"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Encrypt))]
    public static T ResetEncrypt<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.Encrypt));
    #endregion
    #region GitUsername
    /// <inheritdoc cref="ConfixBuildSettings.GitUsername"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.GitUsername))]
    public static T SetGitUsername<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.GitUsername, v));
    /// <inheritdoc cref="ConfixBuildSettings.GitUsername"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.GitUsername))]
    public static T ResetGitUsername<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.GitUsername));
    #endregion
    #region GitToken
    /// <inheritdoc cref="ConfixBuildSettings.GitToken"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.GitToken))]
    public static T SetGitToken<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.GitToken, v));
    /// <inheritdoc cref="ConfixBuildSettings.GitToken"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.GitToken))]
    public static T ResetGitToken<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.GitToken));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixBuildSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixBuildSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixBuildSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixBuildSettings), Property = nameof(ConfixBuildSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixBuildSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixRestoreSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixRestoreSettingsExtensions
{
    #region DotnetConfiguration
    /// <inheritdoc cref="ConfixRestoreSettings.DotnetConfiguration"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.DotnetConfiguration))]
    public static T SetDotnetConfiguration<T>(this T o, string v) where T : ConfixRestoreSettings => o.Modify(b => b.Set(() => o.DotnetConfiguration, v));
    /// <inheritdoc cref="ConfixRestoreSettings.DotnetConfiguration"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.DotnetConfiguration))]
    public static T ResetDotnetConfiguration<T>(this T o) where T : ConfixRestoreSettings => o.Modify(b => b.Remove(() => o.DotnetConfiguration));
    #endregion
    #region Environment
    /// <inheritdoc cref="ConfixRestoreSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixRestoreSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixRestoreSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixRestoreSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixRestoreSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixRestoreSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixRestoreSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixRestoreSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixRestoreSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixRestoreSettings), Property = nameof(ConfixRestoreSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixRestoreSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixValidateSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixValidateSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixValidateSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixValidateSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixValidateSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixValidateSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixValidateSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixValidateSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixValidateSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixValidateSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixValidateSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixValidateSettings), Property = nameof(ConfixValidateSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixValidateSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixEncryptSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixEncryptSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixEncryptSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixEncryptSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixEncryptSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixEncryptSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixEncryptSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixEncryptSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixEncryptSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixEncryptSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region InputFile
    /// <inheritdoc cref="ConfixEncryptSettings.InputFile"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.InputFile))]
    public static T SetInputFile<T>(this T o, string v) where T : ConfixEncryptSettings => o.Modify(b => b.Set(() => o.InputFile, v));
    /// <inheritdoc cref="ConfixEncryptSettings.InputFile"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.InputFile))]
    public static T ResetInputFile<T>(this T o) where T : ConfixEncryptSettings => o.Modify(b => b.Remove(() => o.InputFile));
    #endregion
    #region OutFile
    /// <inheritdoc cref="ConfixEncryptSettings.OutFile"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.OutFile))]
    public static T SetOutFile<T>(this T o, string v) where T : ConfixEncryptSettings => o.Modify(b => b.Set(() => o.OutFile, v));
    /// <inheritdoc cref="ConfixEncryptSettings.OutFile"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.OutFile))]
    public static T ResetOutFile<T>(this T o) where T : ConfixEncryptSettings => o.Modify(b => b.Remove(() => o.OutFile));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixEncryptSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixEncryptSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixEncryptSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixEncryptSettings), Property = nameof(ConfixEncryptSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixEncryptSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixDecryptSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixDecryptSettingsExtensions
{
    #region Environment
    /// <inheritdoc cref="ConfixDecryptSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Environment))]
    public static T SetEnvironment<T>(this T o, string v) where T : ConfixDecryptSettings => o.Modify(b => b.Set(() => o.Environment, v));
    /// <inheritdoc cref="ConfixDecryptSettings.Environment"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Environment))]
    public static T ResetEnvironment<T>(this T o) where T : ConfixDecryptSettings => o.Modify(b => b.Remove(() => o.Environment));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixDecryptSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixDecryptSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixDecryptSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixDecryptSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region InputFile
    /// <inheritdoc cref="ConfixDecryptSettings.InputFile"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.InputFile))]
    public static T SetInputFile<T>(this T o, string v) where T : ConfixDecryptSettings => o.Modify(b => b.Set(() => o.InputFile, v));
    /// <inheritdoc cref="ConfixDecryptSettings.InputFile"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.InputFile))]
    public static T ResetInputFile<T>(this T o) where T : ConfixDecryptSettings => o.Modify(b => b.Remove(() => o.InputFile));
    #endregion
    #region OutFile
    /// <inheritdoc cref="ConfixDecryptSettings.OutFile"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.OutFile))]
    public static T SetOutFile<T>(this T o, string v) where T : ConfixDecryptSettings => o.Modify(b => b.Set(() => o.OutFile, v));
    /// <inheritdoc cref="ConfixDecryptSettings.OutFile"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.OutFile))]
    public static T ResetOutFile<T>(this T o) where T : ConfixDecryptSettings => o.Modify(b => b.Remove(() => o.OutFile));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixDecryptSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixDecryptSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixDecryptSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixDecryptSettings), Property = nameof(ConfixDecryptSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixDecryptSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixConfigShowSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigShowSettingsExtensions
{
    #region Format
    /// <inheritdoc cref="ConfixConfigShowSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Format))]
    public static T SetFormat<T>(this T o, string v) where T : ConfixConfigShowSettings => o.Modify(b => b.Set(() => o.Format, v));
    /// <inheritdoc cref="ConfixConfigShowSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Format))]
    public static T ResetFormat<T>(this T o) where T : ConfixConfigShowSettings => o.Modify(b => b.Remove(() => o.Format));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixConfigShowSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixConfigShowSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixConfigShowSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixConfigShowSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixConfigShowSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixConfigShowSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixConfigShowSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigShowSettings), Property = nameof(ConfixConfigShowSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixConfigShowSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixConfigSetSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigSetSettingsExtensions
{
    #region Verbosity
    /// <inheritdoc cref="ConfixConfigSetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixConfigSetSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixConfigSetSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixConfigSetSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Path
    /// <inheritdoc cref="ConfixConfigSetSettings.Path"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Path))]
    public static T SetPath<T>(this T o, string v) where T : ConfixConfigSetSettings => o.Modify(b => b.Set(() => o.Path, v));
    /// <inheritdoc cref="ConfixConfigSetSettings.Path"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Path))]
    public static T ResetPath<T>(this T o) where T : ConfixConfigSetSettings => o.Modify(b => b.Remove(() => o.Path));
    #endregion
    #region Value
    /// <inheritdoc cref="ConfixConfigSetSettings.Value"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Value))]
    public static T SetValue<T>(this T o, string v) where T : ConfixConfigSetSettings => o.Modify(b => b.Set(() => o.Value, v));
    /// <inheritdoc cref="ConfixConfigSetSettings.Value"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Value))]
    public static T ResetValue<T>(this T o) where T : ConfixConfigSetSettings => o.Modify(b => b.Remove(() => o.Value));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixConfigSetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixConfigSetSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixConfigSetSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigSetSettings), Property = nameof(ConfixConfigSetSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixConfigSetSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
#region ConfixConfigListSettingsExtensions
/// <summary>Used within <see cref="ConfixTasks"/>.</summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class ConfixConfigListSettingsExtensions
{
    #region Format
    /// <inheritdoc cref="ConfixConfigListSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Format))]
    public static T SetFormat<T>(this T o, string v) where T : ConfixConfigListSettings => o.Modify(b => b.Set(() => o.Format, v));
    /// <inheritdoc cref="ConfixConfigListSettings.Format"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Format))]
    public static T ResetFormat<T>(this T o) where T : ConfixConfigListSettings => o.Modify(b => b.Remove(() => o.Format));
    #endregion
    #region Verbosity
    /// <inheritdoc cref="ConfixConfigListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Verbosity))]
    public static T SetVerbosity<T>(this T o, string v) where T : ConfixConfigListSettings => o.Modify(b => b.Set(() => o.Verbosity, v));
    /// <inheritdoc cref="ConfixConfigListSettings.Verbosity"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Verbosity))]
    public static T ResetVerbosity<T>(this T o) where T : ConfixConfigListSettings => o.Modify(b => b.Remove(() => o.Verbosity));
    #endregion
    #region Framework
    /// <inheritdoc cref="ConfixConfigListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Framework))]
    public static T SetFramework<T>(this T o, string v) where T : ConfixConfigListSettings => o.Modify(b => b.Set(() => o.Framework, v));
    /// <inheritdoc cref="ConfixConfigListSettings.Framework"/>
    [Pure] [Builder(Type = typeof(ConfixConfigListSettings), Property = nameof(ConfixConfigListSettings.Framework))]
    public static T ResetFramework<T>(this T o) where T : ConfixConfigListSettings => o.Modify(b => b.Remove(() => o.Framework));
    #endregion
}
#endregion
