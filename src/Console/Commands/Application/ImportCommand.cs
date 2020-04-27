using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Application
{
    [Command(Name = "import", Description = "Import application data.")]
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
        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to export.")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to export.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;
        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the file.")]
        public string Path { get; set; }

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Console.WriteLine($"{nameof(Path)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!File.Exists(Path))
            {
                Console.WriteLine($"The value of --path parameters \"{Path}\" is not a valid file.");
                return (int)StatusCodes.InvalidArgument;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _httpClient.WithSubscription(sourceSettings);

            // TODO

            Console.WriteLine($"Successfully imported data to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private async Task ProcessFile()
        {

            int numberOfSheets = 10;
            using (var progressBar = new ProgressBar(numberOfSheets, "main progressbar"))
            {


                var definition = "Customer";
                var dataSource = "Default"; //TODO : How to know the DS?


                await CreateEntities(progressBar, _httpClient, Tenant, Environment, definition, dataSource, new List<IDictionary<string, object>>()
                {
                  new Dictionary<string,object>
                  {
                      {"_code", "C1" }
                  }
                });
            }
        }

        private static async Task CreateEntities(ProgressBar progressBar, HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IList<IDictionary<string, object>> data)
        {

            using (var child = progressBar.Spawn(data.Count, "child actions"))
            {

                foreach (var entity in data)
                    _ = await CreateEntity(httpClient, tenantCode, environmentCode, definition, dataSource, entity);
                child.Tick();
            }


        }

        private static async Task<int> CreateEntity(HttpClient httpClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IDictionary<string, object> data)
        {
            var response = await httpClient.PostAsJsonAsync($"/api/v1/{tenantCode}/{environmentCode}/application/{definition}/{dataSource}", data);
            if (response.IsSuccessStatusCode)
            {
                return (int)StatusCodes.Success;
            }

            var apiError = await GetErrorFromApiResponse(response);

            Console.WriteLine($"{apiError.Code}: {apiError.Message}");

            return (int)StatusCodes.InvalidOperation;
        }

        private static Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => response.Content.ReadAsJsonAsync<ApiError>();


    }
}
