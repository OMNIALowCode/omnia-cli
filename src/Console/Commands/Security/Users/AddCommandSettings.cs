using Spectre.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Security.Users
{
    public class AddCommandSettings : CommandSettings
    {
        [CommandOption("-s|--subscription <VALUE>")]
        [Description("Name of the configured subscription.")]
        public string Subscription { get; set; }

        [CommandOption("-t|--tenant <VALUE>")]
        [Description("Tenant code where the user will be associated.")]
        public string Tenant { get; set; }

        [CommandOption("-e|--environment <VALUE>")]
        [Description("Tenant's environment.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [CommandOption("-u|--username <VALUE>")]
        [Description("Username.")]
        public string Username { get; set; }

        [CommandOption("-r|--role <VALUE>")]
        [Description("Tenant's role to which the user will be associated with.")]
        public string Role { get; set; }
    }
}
