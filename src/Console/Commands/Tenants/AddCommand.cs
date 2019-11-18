using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace Omnia.CLI.Commands.Tenants
{
    [Command(Name = "add", Description = "Create a new Tenant.")]
    [HelpOption("-h|--help")]
    public class AddCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public AddCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
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
                Console.WriteLine($"Subscription {Subscription} can't be found.");
                return (int)StatusCodes.InvalidOperation;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _httpClient.WithSubscription(sourceSettings);

            await CreateTenant(_httpClient, Code, Name);

            return (int) StatusCodes.Success;
        }

        private static async Task<int> CreateTenant(HttpClient httpClient, string tenantCode, string tenantName)
        {
            var response = await httpClient.PostAsJsonAsync($"/api/v1/management/tenants", new { Code = tenantCode, Name = tenantName });
            if (response.IsSuccessStatusCode)
                return (int)StatusCodes.Success;

            var apiError = await GetErrorFromApiResponse(response);

            Console.WriteLine($"{apiError.Code}: {apiError.Message}");

            return (int)StatusCodes.InvalidOperation;
        }

        private static async Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

        private class ApiError
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }
}