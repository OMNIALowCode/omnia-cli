using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Management.Tenants
{
    [Command(Name = "add", Description = "Create a new Tenant.")]
    [HelpOption("-h|--help")]
    public class AddCommand
    {
        private readonly IApiClient _apiClient;
        private readonly AppSettings _settings;

        public AddCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _settings = options.Value;
        }
    
        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }
        [Option("--code", CommandOptionType.SingleValue, Description = "Code of the Tenant to create.")]
        public string Code { get; set; }
        [Option("--name", CommandOptionType.SingleValue, Description = "Name of the Tenant to create.")]
        public string Name { get; set; }
        

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrWhiteSpace(Subscription))
            {
                Console.WriteLine($"{nameof(Subscription)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                Console.WriteLine($"{nameof(Code)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Name))
                Name = Code;

            if (!_settings.Exists(Subscription))
            {
                Console.WriteLine($"Subscription \"{Subscription}\" can't be found.");
                return (int)StatusCodes.InvalidOperation;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _apiClient.Authenticate(sourceSettings);

            return await CreateTenant(_apiClient, Code, Name);
        }

        private static async Task<int> CreateTenant(IApiClient apiClient, string tenantCode, string tenantName)
        {
            var response = await apiClient.Post($"/api/v1/management/tenants", new { Code = tenantCode, Name = tenantName }.ToHttpStringContent());
            if (response.Success)
            {
                Console.WriteLine($"Tenant \"{tenantName}\" ({tenantCode}) created successfully.");
                return (int)StatusCodes.Success;
            }

            Console.WriteLine($"{response.ErrorDetails.Code}: {response.ErrorDetails.Message}");

            return (int)StatusCodes.InvalidOperation;
        }
    }
}