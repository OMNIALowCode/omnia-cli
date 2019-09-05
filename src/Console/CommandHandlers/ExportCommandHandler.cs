using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

 namespace Omnia.CLI.CommandHandlers
{
    public static class ExportCommandHandler
    {
        public static async Task Run(AppSettings settings, string subscription, string tenantCode, string environmentCode)
        {
            var sourceSettings = settings.Subscriptions.FirstOrDefault(s => s.Name.Equals(subscription));
            if (sourceSettings == null)
                throw new InvalidOperationException($"Can't find subscription {subscription}");

            
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

            await DownloadTemplate(httpClient, tenantCode, environmentCode, Path.Combine(path, "model"));

            var currentBuildVersion = await CurrentBuildNumber(httpClient, tenantCode, environmentCode);

            await DownloadBuild(httpClient, tenantCode, environmentCode, currentBuildVersion, Path.Combine(path, "src"));
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
