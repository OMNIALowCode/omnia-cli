using McMaster.Extensions.CommandLineUtils;
using System;

namespace Omnia.CLI.Commands.Application
{
    [Command(Name = "application", Description = "Commands related to Tenant.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(ImportCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
