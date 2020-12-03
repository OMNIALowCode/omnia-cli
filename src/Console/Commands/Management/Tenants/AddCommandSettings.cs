using Spectre.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Management.Tenants
{
    public sealed class AddCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]
        public string Subscription { get; set; }

        [CommandOption("-c|--code <VALUE>")]
        [Description("Code of the Tenant to create.")]
        public string Code { get; set; }

        [CommandOption("-n|--name <VALUE>")]
        [Description("Name of the Tenant to create.")]
        public string Name { get; set; }
    }
}