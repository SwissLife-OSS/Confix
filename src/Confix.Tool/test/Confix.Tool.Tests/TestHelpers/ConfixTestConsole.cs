using System.CommandLine;
using System.CommandLine.IO;
using Snapshooter.Extensions;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;

namespace ConfiX.Inputs;

public sealed class ConfixTestConsole : IAnsiConsole, IDisposable, IConsole
{
    private readonly IAnsiConsole _console;
    private readonly StringWriter _writer;
    private IAnsiConsoleCursor? _cursor;

    /// <inheritdoc/>
    public Profile Profile => _console.Profile;

    /// <inheritdoc/>
    public IExclusivityMode ExclusivityMode => _console.ExclusivityMode;

    /// <summary>
    /// Gets the console input.
    /// </summary>
    public TestConsoleInput Input { get; }

    /// <inheritdoc/>
    public RenderPipeline Pipeline => _console.Pipeline;

    /// <inheritdoc/>
    public IAnsiConsoleCursor Cursor => _cursor ?? _console.Cursor;

    /// <inheritdoc/>
    IAnsiConsoleInput IAnsiConsole.Input => Input;

    /// <summary>
    /// Gets the console output.
    /// </summary>
    public string Output => _writer.ToString();

    /// <summary>
    /// Gets the console output lines.
    /// </summary>
    public IReadOnlyList<string> Lines
        => StringExtension.NormalizeLineEndings(Output).TrimEnd('\n').Split(new char[] { '\n' });

    /// <summary>
    /// Gets or sets a value indicating whether or not VT/ANSI sequences
    /// should be emitted to the console.
    /// </summary>
    public bool EmitAnsiSequences { get; set; }

    public IStandardStreamWriter Out { get; }

    public IStandardStreamWriter Error { get; }

    public bool IsOutputRedirected => false;

    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="System.CommandLine.IO.TestConsole"/> class.
    /// </summary>
    public ConfixTestConsole()
    {
        _writer = new StringWriter();

        Input = new TestConsoleInput();
        EmitAnsiSequences = false;
        Out = new Writer(this);
        Error = new Writer(this);

        _console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = (ColorSystemSupport) ColorSystem.TrueColor,
            Out = new AnsiConsoleOutput(_writer),
            Interactive = InteractionSupport.No,
            Enrichment = new ProfileEnrichment { UseDefaultEnrichers = false, },
        });

        _console.Profile.Width = 400;
        _console.Profile.Height = 24;
        _console.Profile.Capabilities.Ansi = true;
        _console.Profile.Capabilities.Unicode = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _writer.Dispose();
    }

    /// <inheritdoc/>
    public void Clear(bool home)
    {
        _console.Clear(home);
    }

    /// <inheritdoc/>
    public void Write(IRenderable renderable)
    {
        if (EmitAnsiSequences)
        {
            _console.Write(renderable);
        }
        else
        {
            foreach (var segment in renderable.GetSegments(this))
            {
                if (segment.IsControlCode)
                {
                    continue;
                }

                Profile.Out.Writer.Write(segment.Text);
            }
        }
    }

    internal void SetCursor(IAnsiConsoleCursor? cursor)
    {
        _cursor = cursor;
    }

    private sealed class Writer : IStandardStreamWriter
    {
        private readonly ConfixTestConsole _console;

        public Writer(ConfixTestConsole console)
        {
            _console = console;
        }

        public void Write(string? value)
        {
            if (value is { })
            {
                _console.Write(value, null);
            }
        }
    }
}
