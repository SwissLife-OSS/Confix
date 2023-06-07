
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .Build();

Console.WriteLine(configuration["Foo"]);
while (true)
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
    Console.WriteLine(configuration["Foo"]);
}