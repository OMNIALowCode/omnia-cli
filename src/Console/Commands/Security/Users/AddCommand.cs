using System;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Omnia.CLI.Infrastructure;
using Spectre.Cli;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Security.Users
{
    [Description("Associate user to Tenant's role.")]
    public class AddCommand : AsyncCommand<AddCommandSettings>
    {
        private readonly IApiClient _apiClient;
        private readonly AppSettings _settings;
        public AddCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _settings = options.Value;
        }

        public override ValidationResult Validate(CommandContext context, AddCommandSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Subscription))
            {
                return ValidationResult.Error($"{nameof(settings.Subscription)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Tenant))
            {
                return ValidationResult.Error($"{nameof(settings.Tenant)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Username))
            {
                return ValidationResult.Error($"{nameof(settings.Username)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Role))
            {
                return ValidationResult.Error($"{nameof(settings.Role)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Environment))
            {
                return ValidationResult.Error($"{nameof(settings.Environment)} is required");
            }

            if (!_settings.Exists(settings.Subscription))
            {
                return ValidationResult.Error($"Subscription \"{settings.Subscription}\" can't be found.");
            }
            return base.Validate(context, settings);
        }
        public override async Task<int> ExecuteAsync(CommandContext context, AddCommandSettings settings)
        {
            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            return await AddUserToRole(_apiClient, settings.Tenant, settings.Username, settings.Role, settings.Environment).ConfigureAwait(false);
        }

        private static async Task<int> AddUserToRole(IApiClient apiClient, string tenantCode, string _username, string role, string environment)
        {
            var patch = new JsonPatchDocument().Add("/subjects/-", new { username = _username });
            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await apiClient.Patch($"/api/v1/{tenantCode}/{environment}/security/AuthorizationRole/{role}",
                new StringContent(dataAsString, Encoding.UTF8, "application/json")).ConfigureAwait(false);

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
