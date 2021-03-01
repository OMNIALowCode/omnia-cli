using Spectre.Console.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Security.Users
{
    public class ImportCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]

        public string Subscription { get; set; }

        [CommandOption("-t|--tenant <VALUE>")]
        [Description("Import CSV data to the Tenant.")]

        public string Tenant { get; set; }

        [CommandOption("-e|--environment <VALUE>")]
        [Description("Tenant's environment.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [CommandOption("-p|--path <VALUE>")]
        [Description("Complete path to the CSV file.")]
        public string Path { get; set; }
    }
}
