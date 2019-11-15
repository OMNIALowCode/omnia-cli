using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Tenants
{
    [Command(Name = "associate-admin", Description = "Associate admin user to Tenant.")]
    [HelpOption("-h|--help")]
    public class AssociateAdminCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public AssociateAdminCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }
    
        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant where the user will be associated.")]
        public string Tenant { get; set; }
        [Option("--username", CommandOptionType.SingleValue, Description = "Username of administrator.")]
        public string Username { get; set; }
        

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrWhiteSpace(Subscription))
            {
                Console.WriteLine($"{nameof(Subscription)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Tenant))
            {
                Console.WriteLine($"{nameof(Tenant)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                Console.WriteLine($"{nameof(Username)} is required");
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