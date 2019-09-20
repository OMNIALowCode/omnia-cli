using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI
{
    [Command(Name = "omnia-cli", Description = "")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(Omnia.CLI.Commands.Subscriptions.BaseCommand))]
    [Subcommand(typeof(Omnia.CLI.Commands.Model.BaseCommand))]
    [Subcommand(typeof(Omnia.CLI.Commands.Security.BaseCommand))]

    public class App
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}
