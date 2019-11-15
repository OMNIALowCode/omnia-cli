using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace Omnia.CLI
{
    [Command(Name = "omnia-cli", Description = "OMNIA Platform CLI")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(Omnia.CLI.Commands.Subscriptions.BaseCommand))]
    [Subcommand(typeof(Omnia.CLI.Commands.Model.BaseCommand))]
    [Subcommand(typeof(Omnia.CLI.Commands.Security.BaseCommand))]
    [Subcommand(typeof(Omnia.CLI.Commands.Tenants.BaseCommand))]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]

    public class App
    {
        public void OnExecute(CommandLineApplication app)
        {

        }

        private static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}
