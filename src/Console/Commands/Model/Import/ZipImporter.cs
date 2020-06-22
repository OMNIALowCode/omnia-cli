using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Import
{
    internal class ZipImporter
    {
        private readonly HttpClient _httpClient;

        public ZipImporter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int> ImportAndBuild(string tenant, string environment, string path)
        {
            await Import(tenant, environment, path);

            return await BuildModel(_httpClient, tenant, environment);
        }


        public async Task<int> Import(string tenant, string environment, string path)
        {

            await UploadModel(_httpClient, tenant, environment, path);

            return (int)StatusCodes.Success;
        }


        private static async Task UploadModel(HttpClient httpClient, string tenantCode, string environmentCode, string path)
        {
            using var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture))
                {
                    {ReadFileAsStream(path), "file", Path.GetFileName(path)}
                };

            using var response = await httpClient.PostAsync($"/api/v1/{tenantCode}/{environmentCode}/model/import", content);
            response.EnsureSuccessStatusCode();
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
