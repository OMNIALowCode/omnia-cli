using McMaster.Extensions.CommandLineUtils;
using System;

namespace Omnia.CLI.Commands.Management
{
    [Command(Name = "management", Description = "Commands related to Management.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(Omnia.CLI.Commands.Management.Tenants.BaseCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
