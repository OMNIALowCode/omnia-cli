using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;

namespace Omnia.CLI.Commands.Security.Users
{
    [Command(Name = "add", Description = "Associate user to Tenant's role.")]
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
        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant code where the user will be associated.")]
        public string Tenant { get; set; }
        [Option("--username", CommandOptionType.SingleValue, Description = "Username.")]
        public string Username { get; set; }
        [Option("--role", CommandOptionType.SingleValue, Description = "Tenant's role to which the user will be associated with.")]
        public string Role { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "Tenant's environment.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;


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

            if (string.IsNullOrWhiteSpace(Role))
            {
                Console.WriteLine($"{nameof(Role)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (string.IsNullOrWhiteSpace(Environment))
            {
                Console.WriteLine($"{nameof(Environment)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!_settings.Exists(Subscription))
            {
                Console.WriteLine($"Subscription \"{Subscription}\" can't be found.");
                return (int)StatusCodes.InvalidOperation;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);
            
            await _httpClient.WithSubscription(sourceSettings);

            return await AddUserToRole(_httpClient, Tenant, Username, Role, Environment);
        }

        private static async Task<int> AddUserToRole(HttpClient httpClient, string tenantCode, string _username, string role, string environment)
        {
            var patch = new JsonPatchDocument().Add("/subjects/-", new { username = _username });

            var response = await httpClient.PatchAsJsonAsync($"/api/v1/{tenantCode}/{environment}/security/AuthorizationRole/{role}", patch);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"User \"{_username}\" associated to {tenantCode} {role} role successfully.");
                return (int)StatusCodes.Success;
            }

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
 