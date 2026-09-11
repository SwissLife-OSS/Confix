using System.ComponentModel.DataAnnotations;
using Confix;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var services = new ServiceCollection();
services.AddConfixOptions<SmtpOptions>(configuration);
using var provider = services.BuildServiceProvider();
provider.GetRequiredService<IStartupValidator>().Validate();
Console.WriteLine("Application started with valid configuration.");

[ConfixSection("Messaging:Smtp")]
public sealed class SmtpOptions
{
    [Required]
    public string Host { get; set; } = "";
    [Range(1, 65535)]
    public int Port { get; set; } = 587;
    [Required, EmailAddress]
    public string Sender { get; set; } = "";
}
