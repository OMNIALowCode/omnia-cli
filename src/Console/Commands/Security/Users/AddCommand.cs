using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Security.Users
{
    [Command(Name = "add", Description = "Associate user to Tenant's role.")]
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

            await _apiClient.Authenticate(sourceSettings);

            return await AddUserToRole(_apiClient, Tenant, Username, Role, Environment);
        }

        private static async Task<int> AddUserToRole(IApiClient apiClient, string tenantCode, string _username, string role, string environment)
        {
            var patch = new JsonPatchDocument().Add("/subjects/-", new { username = _username });
            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await apiClient.Patch($"/api/v1/{tenantCode}/{environment}/security/AuthorizationRole/{role}",
                new StringContent(dataAsString, Encoding.UTF8, "application/json"));

            if (response.Success)
            {
                Console.WriteLine($"User \"{_username}\" associated to {tenantCode} {role} role successfully.");
                return (int)StatusCodes.Success;
            }

            Console.WriteLine($"{response.ErrorDetails.Code}: {response.ErrorDetails.Message}");

            return (int)StatusCodes.InvalidOperation;
        }
    }
}
