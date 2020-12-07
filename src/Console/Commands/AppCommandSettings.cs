using Spectre.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands
{
    public sealed class AppCommandSettings : CommandSettings
    {
        [CommandOption("-v|--version")]
        [Description("Display the CLI version.")]
        public bool Version { get; set; }
    }
}
