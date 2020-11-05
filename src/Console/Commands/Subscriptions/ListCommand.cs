using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "list", Description = "List the subscriptions configured.")]
    [HelpOption("-h|--help")]
    public class ListCommand
    {
        private readonly AppSettings _settings;
        public ListCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public Task<int> OnExecute(CommandLineApplication cmd)
        {
            foreach (var subscription in _settings.Subscriptions)
            {
                Console.WriteLine($"{subscription.Name} ({subscription.Endpoint})");
            }

            return Task.FromResult((int)StatusCodes.Success);
        }

    }
}
