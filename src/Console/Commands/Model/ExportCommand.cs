using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model
{
    [Command(Name = "export", Description = "Export Tenant model and last build to the current folder.")]
    [HelpOption("-h|--help")]
    public class ExportCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        public ExportCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory)
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
        

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            var sourceSettings = _settings.GetSubscription(Subscription);
            
            var path = Directory.GetCurrentDirectory();

            await _httpClient.WithSubscription(sourceSettings);

            await DownloadModel(_httpClient, Tenant, Environment, Path.Combine(path, "model"));

            var currentBuildVersion = await CurrentBuildNumber(_httpClient, Tenant, Environment);

            await DownloadBuild(_httpClient, Tenant, Environment, currentBuildVersion, Path.Combine(path, "src"));

            return (int) StatusCodes.Success;
        }

        private static async Task DownloadModel(HttpClient httpClient, string tenantCode, string environmentCode, string path)
        {
            var response = await httpClient.GetAsync($"/api/v1/{tenantCode}/{environmentCode}/model/export");
            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (var archive = new ZipArchive(responseStream))
                {
                    archive.ExtractToDirectory(path, true);
                }
            }
        }

        private static async Task<string> CurrentBuildNumber(HttpClient httpClient, string tenantCode, string environmentCode)
        {
            var response = await httpClient.GetAsync($"/api/v1/{tenantCode}/{environmentCode}/model/builds?pageSize=1");
            response.EnsureSuccessStatusCode();
            var buildData = await response.Content.ReadAsJsonAsync<List<BuildData>>();
            return buildData.First().BuildVersion;
        }

        private static async Task DownloadBuild(HttpClient httpClient, string tenantCode, string environmentCode, string version, string path)
        {
            var response = await httpClient.GetAsync($"/api/v1/{tenantCode}/{environmentCode}/model/builds/{version}/download");
            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (var archive = new ZipArchive(responseStream))
                {
                    archive.ExtractToDirectory(path, true);
                }
            }
        }

        internal class BuildData
        {
            public string BuildVersion { get; set; }
            public string State { get; set; }

        }

    }
}
