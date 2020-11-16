using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model.Import
{
    [Command(Name = "import", Description = "Import model from .zip file to Tenant.")]
    [HelpOption("-h|--help")]
    public class ImportCommand
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

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }

        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to export.")]
        public string Tenant { get; set; }

        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to import.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;

        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the ZIP file.")]
        public string Path { get; set; }

        [Option("--build", CommandOptionType.NoValue, Description = "Perform a model build after the importation.")]
        public bool Build { get; set; }

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

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            if (Build)
                return await _zipImporter.ImportAndBuild(Tenant, Environment, Path).ConfigureAwait(false);

            return await _zipImporter.Import(Tenant, Environment, Path).ConfigureAwait(false);

        }


    }
}
