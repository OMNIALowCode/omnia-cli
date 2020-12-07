using System.ComponentModel;
using Spectre.Cli;

namespace Omnia.CLI.Commands.Application
{
    public class ImportCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]
        public string Subscription { get; set; }

        [CommandOption("-e|--environment <VALUE>")]
        [Description("Environment to export.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [CommandOption("-t|--tenant <VALUE>")]
        [Description("Tenant to export.")]
        public string Tenant { get; set; }

        [CommandOption("-p|--path <VALUE>")]
        [Description("Complete path to the file.")]
        public string Path { get; set; }
    }
}
