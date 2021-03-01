using Spectre.Console.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Subscriptions
{
    public sealed class RemoveCommandSettings : CommandSettings
    {
        [CommandOption("--name <VALUE>")]
        [Description("Name to reference this subscription configuration when using the CLI.")]
        public string Name { get; set; }

    }
}
