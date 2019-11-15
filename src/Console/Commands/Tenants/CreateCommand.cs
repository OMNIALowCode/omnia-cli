using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Tenants
{
    [Command(Name = "create", Description = "Create new Tenant.")]
    [HelpOption("-h|--help")]
    public class CreateCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public CreateCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }
    
        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }
        [Option("--name", CommandOptionType.SingleValue, Description = "Name of the Tenant to create.")]
        public string Name { get; set; }
        

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrWhiteSpace(Subscription))
            {
                Console.WriteLine($"{nameof(Subscription)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                Console.WriteLine($"{nameof(Name)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!_settings.Exists(Subscription))
            {
                Console.WriteLine($"Subscription {Subscription} can't be found.");
                return (int)StatusCodes.InvalidOperation;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);
            
            var path = Directory.GetCurrentDirectory();

            await _httpClient.WithSubscription(sourceSettings);
           
            return (int) StatusCodes.Success;
        }
    }
}