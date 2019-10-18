using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI.Commands.Security.Users
{
    [Command(Name = "users", Description = "Commands related to Tenant Users security.")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(ImportCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}
