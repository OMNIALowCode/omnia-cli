using McMaster.Extensions.CommandLineUtils;
using System;

namespace Omnia.CLI.Commands.Security
{
    [Command(Name = "security", Description = "Commands related to Tenant Security.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(Omnia.CLI.Commands.Security.Users.BaseCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
