using CsvHelper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Security.Users
{
    [Description(@"Import a CSV to assign users to a given tenant role. CSV example:

---
        username,role
        username@domain.com,Manager
        admin@domain.com,Administration
---
")]
    public sealed class ImportCommand : AsyncCommand<ImportCommandSettings>
    {
        private readonly IApiClient _apiClient;
        private readonly AppSettings _settings;

        public ImportCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _settings = options.Value;
        }

        public override ValidationResult Validate(CommandContext context, ImportCommandSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Subscription))
            {
                return ValidationResult.Error($"{nameof(settings.Subscription)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Tenant))
            {
                return ValidationResult.Error($"{nameof(settings.Tenant)} is required");
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

        public override async Task<int> ExecuteAsync(CommandContext context, ImportCommandSettings settings)
        {
            var entries = ParseFile(settings.Path);

            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            var tasks = entries
                .GroupBy(r => r.Role, StringComparer.InvariantCultureIgnoreCase)
                .Select(role =>
                    UpdateRole(_apiClient, role.Key, role.Select(c => c.Username).ToList(), settings.Tenant, settings.Environment)
                    );

            await Task.WhenAll(tasks).ConfigureAwait(false);

            Console.WriteLine($"Users imported to tenant \"{settings.Tenant}\" successfully.");
            return (int)StatusCodes.Success;
        }

        private async Task UpdateRole(IApiClient apiClient, string role, IEnumerable<string> usernames, string tenant, string environment)
        {
            var patch = new JsonPatchDocument();

            foreach (var user in usernames)
                patch.Add("/subjects/-", new { username = user });

            var dataAsString = JsonConvert.SerializeObject(patch);

            await apiClient.Patch($"/api/v1/{tenant}/{environment}/security/AuthorizationRole/{role}",
                new StringContent(dataAsString, Encoding.UTF8, "application/json")).ConfigureAwait(false);

        }

        private static IEnumerable<CsvEntry> ParseFile(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ",";

            // Ignore header case.
            csv.Configuration.PrepareHeaderForMatch = (header, _) => header.ToLower();
            return csv.GetRecords<CsvEntry>().ToList();
        }

        private class CsvEntry
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }
    }
}
