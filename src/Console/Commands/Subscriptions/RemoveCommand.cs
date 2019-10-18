using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

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
            var subscription = _settings.GetSubscription(Name);
            _settings.Subscriptions.Remove(subscription);

            using (var file = File.CreateText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            return Task.FromResult((int)StatusCodes.Success);
        }

    }
}
