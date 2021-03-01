using Microsoft.Extensions.Options;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model
{
    [Description("Export Tenant model and last build to the current folder.")]
    public sealed class ExportCommand : AsyncCommand<ExportCommandSettings>
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;

        public ExportCommand(IOptions<AppSettings> options,
            IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
        }

        public override ValidationResult Validate(CommandContext context, ExportCommandSettings settings)
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

            if (string.IsNullOrWhiteSpace(settings.Path))
            {
                return ValidationResult.Error($"{nameof(settings.Path)} is required");
            }

            if (!_settings.Exists(settings.Subscription))
            {
                return ValidationResult.Error($"Subscription \"{settings.Subscription}\" can't be found.");
            }
            return base.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ExportCommandSettings settings)
        {
            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings)
                .ConfigureAwait(false);

            await DownloadModel(_apiClient, settings.Tenant, settings.Environment, System.IO.Path.Combine(settings.Path, "model"))
                .ConfigureAwait(false);

            var currentBuildVersion = await CurrentBuildNumber(_apiClient, settings.Tenant, settings.Environment)
                .ConfigureAwait(false);

            await DownloadBuild(_apiClient, settings.Tenant, settings.Environment, currentBuildVersion, System.IO.Path.Combine(settings.Path, "src"))
                .ConfigureAwait(false);

            Console.WriteLine($"Tenant \"{settings.Tenant}\" model and last build exported successfully.");
            return (int)StatusCodes.Success;
        }

        private static async Task DownloadModel(IApiClient apiClient, string tenantCode, string environmentCode, string path)
        {
            var (ApiDetails, Content) = await apiClient.GetStream($"/api/v1/{tenantCode}/{environmentCode}/model/export")
                .ConfigureAwait(false);
            if (!ApiDetails.Success) return;

            await using var responseStream = Content;
            var archive = new ZipArchive(responseStream);
            archive.ExtractToDirectory(path, true);
        }

        private static async Task<string> CurrentBuildNumber(IApiClient apiClient, string tenantCode, string environmentCode)
        {
            var (ApiDetails, Content) = await apiClient.Get($"/api/v1/{tenantCode}/{environmentCode}/model/builds?pageSize=1")
                .ConfigureAwait(false);
            if (!ApiDetails.Success) return null;

            var buildData = Content.ReadAsJson<List<BuildData>>();
            return buildData.First().BuildVersion;
        }

        private static async Task DownloadBuild(IApiClient apiClient, string tenantCode, string environmentCode, string version, string path)
        {
            var (_, Content) = await apiClient.GetStream($"/api/v1/{tenantCode}/{environmentCode}/model/builds/{version}/download")
                .ConfigureAwait(false);

            await using var responseStream = Content;
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
