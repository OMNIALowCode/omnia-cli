using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Commands.Model.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    [Command(Name = "apply", Description = "Apply behaviours to model from source code.")]
    [HelpOption("-h|--help")]
    public class ApplyCommand
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;
        private readonly DefinitionService _definitionService;
        private readonly BehaviourReader _reader = new BehaviourReader();
        public ApplyCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
            _definitionService = new DefinitionService(_apiClient);
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

            if (!Directory.Exists(Path))
            {
                Console.WriteLine($"The value of --path parameters \"{Path}\" is not a valid directory.");
                return (int)StatusCodes.InvalidArgument;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _apiClient.Authenticate(sourceSettings);


            var files = Directory.GetFiles(Path, "*.Operations.cs", SearchOption.AllDirectories);

            var processFileTasks = files.Select(ProcessFile);

            await Task.WhenAll(processFileTasks);

            if (Build)
                await _apiClient.BuildModel(Tenant, Environment).ConfigureAwait(false);


            Console.WriteLine($"Successfully applyed behaviours to tenant \"{Tenant}\" model.");
            return (int)StatusCodes.Success;
        }

        private async Task ProcessFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            var entity = _reader.ExtractData(content);

            if (entity.Behaviours.Count > 0 || entity.Usings.Count > 0)
                await ReplaceData(filepath, entity).ConfigureAwait(false);
        }

        private async Task ReplaceData(string filepath, Data.Entity entity)
        {
            bool replacedWithSuccess = await _definitionService.ReplaceData(Tenant, Environment,
                            ExtractEntityFromFileName(filepath), entity).ConfigureAwait(false);
            if (!replacedWithSuccess)
                Console.WriteLine($"Failed to apply behaviours from file {filepath}.");
        }

        private static string ExtractEntityFromFileName(string filepath)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            return filename.Substring(0, filename.Length - ".Operations.cs".Length);
        }
        private static async Task<string> ReadFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sr = new StreamReader(fs);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }

    }
}
