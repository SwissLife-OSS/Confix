using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

public class SpectreStatusContext : IStatus
{
    private readonly StatusContext _context;

    public SpectreStatusContext(StatusContext context)
    {
        _context = context;
    }

    public string Status { get => _context.Status; set => _context.Status = value; }
}