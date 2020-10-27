using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            foreach (var file in Directory.GetFiles(Path, "*.Operations.cs", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Processing file {file}...");
                var content = await ReadFile(file).ConfigureAwait(false);

                var operations = _reader.ExtractMethods(content);

                if (operations.Count == 0) continue;

                await _definitionService.ReplaceBehaviours(Tenant, Environment,
                "Agent", //TODO: DISCOVER ENTITY TYPE
                 ExtractEntityFromFileName(file), operations).ConfigureAwait(false);


            }

            Console.WriteLine($"Successfully applyed behaviours to tenant \"{Tenant}\" model.");
            return (int)StatusCodes.Success;
        }
        private static string ExtractEntityFromFileName(string filepath)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            return filename.Substring(0, filename.Length - ".Operations.cs".Length - 2);
        }
        private static Task<string> ReadFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sr = new StreamReader(fs);
            return sr.ReadToEndAsync();
        }
    }
}
