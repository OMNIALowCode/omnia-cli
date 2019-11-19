using CsvHelper;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public ImportCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
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

            await _httpClient.WithSubscription(sourceSettings);

            var tasks = entries
                .GroupBy(r => r.Role, StringComparer.InvariantCultureIgnoreCase)
                .Select(role => 
                    UpdateRole(_httpClient, role.Key, role.Select(c => c.Username).ToList())
                    );

            await Task.WhenAll(tasks);

            Console.WriteLine($"Users imported to tenant \"{Tenant}\" successfully.");
            return (int)StatusCodes.Success;
        }

        private async Task UpdateRole(HttpClient httpClient, string role, IEnumerable<string> usernames)
        {
            var patch = new JsonPatchDocument();

            foreach (var user in usernames)
                patch.Add("/subjects/-", new { username = user });

            var response = await httpClient.PatchAsJsonAsync($"/api/v1/{Tenant}/{Environment}/security/AuthorizationRole/{role}", patch);
            response.EnsureSuccessStatusCode();
        }

        private static IEnumerable<CsvEntry> ParseFile(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ",";
                csv.Configuration.CultureInfo = CultureInfo.InvariantCulture;
                // Ignore header case.
                csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
                return csv.GetRecords<CsvEntry>().ToList();
            }   
        }

        private class CsvEntry
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }

    }
}
