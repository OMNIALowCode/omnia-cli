using McMaster.Extensions.CommandLineUtils;
using System;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "subscriptions", Description = "Commands to configure subscriptions.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(AddCommand))]
    [Subcommand(typeof(RemoveCommand))]
    [Subcommand(typeof(ListCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
