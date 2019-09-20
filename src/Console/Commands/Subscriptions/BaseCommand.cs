using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "subscriptions", Description = "")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(AddCommand))]
    [Subcommand(typeof(RemoveCommand))]
    public class BaseCommand
    {
        public void OnExecute(CommandLineApplication app)
        {

        }
    }
}
