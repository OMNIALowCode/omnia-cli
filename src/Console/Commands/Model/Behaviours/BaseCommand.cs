using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    [Command(Name = "behaviours", Description = "Commands related to Tenant Model Behaviours.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(ApplyCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Use -h or --help to know how to use it");
        }
    }
}
