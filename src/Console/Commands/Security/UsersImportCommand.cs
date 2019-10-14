﻿using CsvHelper;
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

namespace Omnia.CLI.Commands.Security
{
    [Command(Name = "users-import", Description = "")]
    [HelpOption("-h|--help")]
    public class UsersImportCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public UsersImportCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "")]
        public string Environment { get; set; }

        [Option("--path", CommandOptionType.SingleValue, Description = "")]
        public string Path { get; set; }

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            var entries = ParseFile(Path);

            //TODO: Move to a different class
            var sourceSettings = _settings.GetSubscription(Subscription);

            await _httpClient.WithSubscription(sourceSettings);

            var tasks = entries
                .GroupBy(r => r.Role, StringComparer.InvariantCultureIgnoreCase)
                .Select(role => 
                    UpdateRole(_httpClient, role.Key, role.Select(c => c.Username).ToList())
                    );

            await Task.WhenAll(tasks);
            return 0;
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
