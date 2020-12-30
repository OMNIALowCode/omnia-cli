using Spectre.Console.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Model.Apply
{
    public sealed class ApplyCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]
        public string Subscription { get; set; }

        [CommandOption("-t|--tenant <VALUE>")]
        [Description("Tenant to import.")]
        public string Tenant { get; set; }

        [CommandOption("-e|--environment <VALUE>")]
        [Description("Environment to import.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [CommandOption("-p|--path <VALUE>")]
        [Description("Complete path to the source code directory.")]
        public string Path { get; set; } = ".";

        [CommandOption("-b|--build")]
        [Description("Perform a model build after applying.")]
        public bool Build { get; set; }
    }
}
