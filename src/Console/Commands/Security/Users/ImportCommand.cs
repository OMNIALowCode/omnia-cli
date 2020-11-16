using CsvHelper;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Security.Users
{
    [Command(Name = "import",
        Description = @"Import a CSV to assign users to a given tenant role. CSV example:

---
        username,role
        username@domain.com,Manager
        admin@domain.com,Administration
---
")]
    [HelpOption("-h|--help")]
    public class ImportCommand
    {
        private readonly IApiClient _apiClient;
        private readonly AppSettings _settings;

        public ImportCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _settings = options.Value;
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }

        [Option("--tenant", CommandOptionType.SingleValue, Description = "Import CSV data to the Tenant.")]
        public string Tenant { get; set; }

        [Option("--environment", CommandOptionType.SingleValue, Description = "Tenant Environment.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the CSV file.")]
        public string Path { get; set; }

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

            var entries = ParseFile(Path);

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _apiClient.Authenticate(sourceSettings);

            var tasks = entries
                .GroupBy(r => r.Role, StringComparer.InvariantCultureIgnoreCase)
                .Select(role =>
                    UpdateRole(_apiClient, role.Key, role.Select(c => c.Username).ToList())
                    );

            await Task.WhenAll(tasks);

            Console.WriteLine($"Users imported to tenant \"{Tenant}\" successfully.");
            return (int)StatusCodes.Success;
        }

        private async Task UpdateRole(IApiClient apiClient, string role, IEnumerable<string> usernames)
        {
            var patch = new JsonPatchDocument();

            foreach (var user in usernames)
                patch.Add("/subjects/-", new { username = user });

            var dataAsString = JsonConvert.SerializeObject(patch);

            await apiClient.Patch($"/api/v1/{Tenant}/{Environment}/security/AuthorizationRole/{role}",
                new StringContent(dataAsString, Encoding.UTF8, "application/json"));
            
        }

        private static IEnumerable<CsvEntry> ParseFile(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ",";

            // Ignore header case.
            csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();
            return csv.GetRecords<CsvEntry>().ToList();
        }

        private class CsvEntry
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }
    }
}
