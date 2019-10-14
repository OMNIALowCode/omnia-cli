using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Omnia.CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var configuration = CreateConfigurationRoot();
            
            var services = new ServiceCollection()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .Configure<AppSettings>(configuration)
                .AddHttpClient()
                .BuildServiceProvider();

            var app = new CommandLineApplication<App>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
            return app.Execute(args);
        }

        private static IConfigurationRoot CreateConfigurationRoot()
            => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OMNIA", "CLI", "appsettings.json"), true)
                .Build();
    }
}
