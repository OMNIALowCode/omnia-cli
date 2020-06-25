using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model
{
    [Command(Name = "export", Description = "Export Tenant model and last build to the current folder.")]
    [HelpOption("-h|--help")]
    public class ExportCommand
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;

        public ExportCommand(IOptions<AppSettings> options,
            IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }

        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to export.")]
        public string Tenant { get; set; }

        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to export.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path where exported folders will be created.")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

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

            if (string.IsNullOrWhiteSpace(Path))
            {
                Console.WriteLine($"{nameof(Path)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!_settings.Exists(Subscription))
            {
                Console.WriteLine($"Subscription \"{Subscription}\" can't be found.");
                return (int)StatusCodes.InvalidOperation;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _apiClient.Authenticate(sourceSettings);

            await DownloadModel(_apiClient, Tenant, Environment, System.IO.Path.Combine(Path, "model"));

            var currentBuildVersion = await CurrentBuildNumber(_apiClient, Tenant, Environment);

            await DownloadBuild(_apiClient, Tenant, Environment, currentBuildVersion, System.IO.Path.Combine(Path, "src"));

            Console.WriteLine($"Tenant \"{Tenant}\" model and last build exported successfully.");
            return (int)StatusCodes.Success;
        }

        private static async Task DownloadModel(IApiClient apiClient, string tenantCode, string environmentCode, string path)
        {
            var response = await apiClient.GetStream($"/api/v1/{tenantCode}/{environmentCode}/model/export");
            if (!response.ApiDetails.Success) return;

            await using var responseStream = response.Content;
            var archive = new ZipArchive(responseStream);
            archive.ExtractToDirectory(path, true);
        }

        private static async Task<string> CurrentBuildNumber(IApiClient apiClient, string tenantCode, string environmentCode)
        {
            var response = await apiClient.Get($"/api/v1/{tenantCode}/{environmentCode}/model/builds?pageSize=1");
            if (!response.ApiDetails.Success) return null;

            var buildData = response.Content.ReadAsJson<List<BuildData>>();
            return buildData.First().BuildVersion;
        }

        private static async Task DownloadBuild(IApiClient apiClient, string tenantCode, string environmentCode, string version, string path)
        {
            var response = await apiClient.GetStream($"/api/v1/{tenantCode}/{environmentCode}/model/builds/{version}/download");

            await using var responseStream = response.Content;
            var archive = new ZipArchive(responseStream);
            archive.ExtractToDirectory(path, true);
        }

        internal class BuildData
        {
            public string BuildVersion { get; set; }
            public string State { get; set; }
        }
    }
}
