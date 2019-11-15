using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI.Commands.Tenants
{
    [Command(Name = "tenants", Description = "Commands related to Tenants Management.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(AddCommand))]
    [Subcommand(typeof(AssociateAdminCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}
