using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix;

public interface IConfixModule
{
    void Configure(IServiceCollection services, IConfiguration configuration);
}

