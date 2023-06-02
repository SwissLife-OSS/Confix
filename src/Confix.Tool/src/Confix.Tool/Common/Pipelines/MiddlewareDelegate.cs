using System.Threading.Tasks;

namespace Confix.Tool.Common.Pipelines;

public delegate Task MiddlewareDelegate(IMiddlewareContext context);
