using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Command(Name = "add", Description = "")]
    [HelpOption("-h|--help")]
    public class AddCommand
    {
        private readonly AppSettings _settings;
        public AddCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        [Option("--name", CommandOptionType.SingleValue, Description = "Name")]
        public string Name { get; set; }
        [Option("--endpoint", CommandOptionType.SingleValue, Description = "")]
        public Uri Endpoint { get; set; }
        [Option("--client-id", CommandOptionType.SingleValue, Description = "")]
        public string ClientId { get; set; }
        [Option("--client-secret", CommandOptionType.SingleValue, Description = "")]
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

            using (var file = File.CreateText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            return Task.FromResult(0);
        }

    }
}
