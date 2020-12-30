using Microsoft.Extensions.Options;
using Omnia.CLI.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model.Import
{
    [Description("Import model from .zip file to Tenant.")]
    public sealed class ImportCommand : AsyncCommand<ImportCommandSettings>
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;
        private readonly ZipImporter _zipImporter;

        public ImportCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _settings = options.Value;
            _zipImporter = new ZipImporter(apiClient);
            _apiClient = apiClient;
        }

        public override ValidationResult Validate(CommandContext context, ImportCommandSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Path))
            {
                return ValidationResult.Error($"{nameof(settings.Path)} is required");
            }

            if (!File.Exists(settings.Path))
            {
                return ValidationResult.Error($"The value of --path parameters \"{settings.Path}\" is not a valid file.");
            }

            return base.Validate(context, settings);
        }
        public override async Task<int> ExecuteAsync(CommandContext context, ImportCommandSettings settings)
        {
            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            if (settings.Build)
                return await _zipImporter.ImportAndBuild(settings.Tenant, settings.Environment, settings.Path).ConfigureAwait(false);

            return await _zipImporter.Import(settings.Tenant, settings.Environment, settings.Path).ConfigureAwait(false);
        }
    }
}
