using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "add", Description = "Add the configuration to a given subscription.")]
    [HelpOption("-h|--help")]
    public class AddCommand
    {
        private readonly AppSettings _settings;
        public AddCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        [Option("--name", CommandOptionType.SingleValue, Description = "Name to reference this subscription configuration when using the CLI.")]
        public string Name { get; set; }
        [Option("--endpoint", CommandOptionType.SingleValue, Description = "Subscription endpoint. Example: https://platform.omnialowcode.com")]
        public Uri Endpoint { get; set; }
        [Option("--client-id", CommandOptionType.SingleValue, Description = "API Client - Id.")]
        public string ClientId { get; set; }
        [Option("--client-secret", CommandOptionType.SingleValue, Description = "API Client - Secret.")]
        public string ClientSecret { get; set; }


        public Task<int> OnExecute(CommandLineApplication cmd)
        {
            _settings.Subscriptions.Add(new AppSettings.Subscription()
            {
                Name = Name,
                Endpoint = Endpoint,
                Client = new AppSettings.Client()
                {
                    Id = ClientId,
                    Secret = ClientSecret
                }
            });

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OMNIA", "CLI");
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var file = File.CreateText(Path.Combine(directory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            return Task.FromResult((int)StatusCodes.Success);
        }

    }
}
