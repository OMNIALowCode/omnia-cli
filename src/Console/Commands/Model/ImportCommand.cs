using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model
{
    [Command(Name = "import", Description = "Import model from .zip file to Tenant.")]
    [HelpOption("-h|--help")]
    public class ImportCommand
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationProvider _authenticationProvider;
        public ImportCommand(IOptions<AppSettings> options, IHttpClientFactory httpClientFactory, IAuthenticationProvider authenticationProvider)
        {
            _authenticationProvider = authenticationProvider;
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
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
        [Option("--watch", CommandOptionType.NoValue, Description = "Watches for file changes in all folders and import them.")]
        public bool Watch { get; set; }

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

            await _authenticationProvider.AuthenticateClient(_httpClient, sourceSettings);

            await UploadModel(_httpClient, Tenant, Environment, Path);

            if (Build)
            {
                return await BuildModel(_httpClient, Tenant, Environment);
            }

            Console.WriteLine($"Successfully imported model to tenant \"{Tenant}\".");
            return (int)StatusCodes.Success;
        }

        private static async Task UploadModel(HttpClient httpClient, string tenantCode, string environmentCode, string path)
        {
            using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                content.Add(ReadFileAsStream(path), "file", System.IO.Path.GetFileName(path));

                using (var response = await httpClient.PostAsync($"/api/v1/{tenantCode}/{environmentCode}/model/import", content))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private static async Task<int> BuildModel(HttpClient httpClient, string tenantCode, string environmentCode)
        {
            
            var requestContent = new StringContent(JsonConvert.SerializeObject(new { Clean = true }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"/api/v1/{tenantCode}/{environmentCode}/model/builds", requestContent);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Successfully imported and built model to tenant \"{tenantCode}\".");
                return (int)StatusCodes.Success;
            }

            var apiError = await GetErrorFromApiResponse(response);

            Console.WriteLine($"{apiError.Code}: {apiError.Message}");

            return (int)StatusCodes.InvalidOperation;
        }

        private static StreamContent ReadFileAsStream(string path)
            => new StreamContent(new MemoryStream(GetFileAsByteArray(path)));

        private static byte[] GetFileAsByteArray(string path)
            => File.ReadAllBytes(path);

        private static async Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
    }
}
