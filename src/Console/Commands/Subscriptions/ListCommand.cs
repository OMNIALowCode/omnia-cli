using Microsoft.Extensions.Options;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Description("List the subscriptions configured.")]
    public sealed class ListCommand : Command
    {
        private readonly AppSettings _settings;
        public ListCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public override int Execute(CommandContext context)
        {
            foreach (var subscription in _settings.Subscriptions)
            {
                Console.WriteLine($"{subscription.Name} ({subscription.Endpoint})");
            }

            return (int)StatusCodes.Success;
        }
    }
}
