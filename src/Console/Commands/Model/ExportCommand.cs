using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.Linq;
using System.IO.Compression;
using Omnia.CLI.Extensions;
using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model
{
    [Command(Name = "export", Description = "")]
    [HelpOption("-h|--help")]
    public class ExportCommand
    {
        private readonly AppSettings _settings;
        public ExportCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "")]
        public string Environment { get; set; }
        

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            var sourceSettings = _settings.GetSubscription(Subscription);
            
            var path = Directory.GetCurrentDirectory();

            var authentication = new Authentication(sourceSettings.IdentityServerUrl,
                sourceSettings.Client.Id,
                sourceSettings.Client.Secret);

            var accessToken = authentication.AuthenticateAsync().Result;
            var authValue = new AuthenticationHeaderValue("Bearer", accessToken);

            var httpClient = new HttpClient()
            {
                BaseAddress = sourceSettings.ApiUrl,
                DefaultRequestHeaders = { Authorization = authValue }
            };

            await DownloadTemplate(httpClient, Tenant, Environment, Path.Combine(path, "model"));

            var currentBuildVersion = await CurrentBuildNumber(httpClient, Tenant, Environment);

            await DownloadBuild(httpClient, Tenant, Environment, currentBuildVersion, Path.Combine(path, "src"));

            return 0;
        }

        private static async Task DownloadTemplate(HttpClient httpClient, string tenantCode, string environmentCode, string path)
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
