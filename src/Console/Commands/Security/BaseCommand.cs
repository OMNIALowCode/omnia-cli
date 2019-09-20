using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI.Commands.Security
{
    [Command(Name = "security", Description = "")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(UsersImportCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}
