using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "remove", Description = "Remove a given subscription configuration.")]
    [HelpOption("-h|--help")]
    public class RemoveCommand
    {
        private readonly AppSettings _settings;
        public RemoveCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        [Option("--name", CommandOptionType.SingleValue, Description = "Name of the subscription to remove.")]
        public string Name { get; set; }

        public Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Console.WriteLine($"{nameof(Name)} is required");
                return Task.FromResult((int)StatusCodes.InvalidArgument);
            }

            if (!_settings.Exists(Name))
            {
                Console.WriteLine($"Subscription \"{Name}\" can't be found.");
                return Task.FromResult((int)StatusCodes.InvalidOperation);
            }

            var subscription = _settings.GetSubscription(Name);
            _settings.Subscriptions.Remove(subscription);

            var directory = SettingsPathFactory.Path();

            using (var file = File.CreateText(Path.Combine(directory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            Console.WriteLine($"Subscription \"{Name}\" configuration removed successfully.");
            return Task.FromResult((int)StatusCodes.Success);
        }

    }
}
