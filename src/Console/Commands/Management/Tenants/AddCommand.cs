using System;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;

namespace Omnia.CLI.Commands.Management.Tenants
{
    [Description("Create a new Tenant.")]
    public sealed class AddCommand : AsyncCommand<AddCommandSettings>
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

            if (string.IsNullOrWhiteSpace(settings.Code))
            {
                return ValidationResult.Error($"{nameof(settings.Code)} is required");
            }

            if (!_settings.Exists(settings.Subscription))
            {
                return ValidationResult.Error($"Subscription \"{settings.Subscription}\" can't be found.");
            }
            return base.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, AddCommandSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Name))
                settings.Name = settings.Code;

            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            return await CreateTenant(_apiClient, settings.Code, settings.Name).ConfigureAwait(false);
        }

        private static async Task<int> CreateTenant(IApiClient apiClient, string tenantCode, string tenantName)
        {
            var response = await apiClient.Post("/api/v1/management/tenants", new { Code = tenantCode, Name = tenantName }.ToHttpStringContent())
                .ConfigureAwait(false);
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