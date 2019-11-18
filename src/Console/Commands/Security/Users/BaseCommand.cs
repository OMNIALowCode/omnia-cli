using McMaster.Extensions.CommandLineUtils;
using System;

namespace Omnia.CLI.Commands.Security.Users
{
    [Command(Name = "users", Description = "Commands related to Tenant Users security.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(AddCommand))]
    [Subcommand(typeof(ImportCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
