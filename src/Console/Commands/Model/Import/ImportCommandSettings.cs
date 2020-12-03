using McMaster.Extensions.CommandLineUtils;
using Spectre.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Model.Import
{
    public sealed class ImportCommandSettings : CommandSettings
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
        [Description("Complete path to the ZIP file.")]
        public string Path { get; set; }

        [CommandOption("-b|--build")]
        [Description("Perform a model build after the importation.")]
        public bool Build { get; set; }
    }
}
