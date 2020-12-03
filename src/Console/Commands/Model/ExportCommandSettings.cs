using McMaster.Extensions.CommandLineUtils;
using Spectre.Cli;
using System.ComponentModel;
using System.IO;

namespace Omnia.CLI.Commands.Model
{
    public sealed class ExportCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]
        public string Subscription { get; set; }

        [CommandOption("-t|--tenant <VALUE>")]
        [Description("Tenant to export.")]
        public string Tenant { get; set; }

        [CommandOption("-e|--environment <VALUE>")]
        [Description("Environment to export.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [CommandOption("-p|--path <VALUE>")]
        [Description("Complete path where exported folders will be created.")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();
    }
}
