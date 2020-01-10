﻿using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI.Commands.Management.Tenants
{
    [Command(Name = "tenants", Description = "Commands related to Tenants Management.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(AddCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}