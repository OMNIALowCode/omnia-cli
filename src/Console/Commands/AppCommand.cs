using Spectre.Cli;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace Omnia.CLI.Commands
{
    public sealed class AppCommand : AsyncCommand<AppCommandSettings>
    {
        public override Task<int> ExecuteAsync(CommandContext context, AppCommandSettings settings)
        {
            if (settings.Version)
                Console.WriteLine(GetVersion());
            else
                Console.WriteLine("Use -h or --help to know how to use it");

            return Task.FromResult((int)StatusCodes.Success);
        }
        private static string GetVersion() => typeof(Program)
                 .Assembly
                 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                 ?.InformationalVersion ?? "0.0.0";
    }
}
