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
        private readonly IApiClient _apiClient;

        public ZipImporter(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<int> ImportAndBuild(string tenant, string environment, string path)
        {
            await Import(tenant, environment, path);

            return await BuildModel(_apiClient, tenant, environment);
        }


        public async Task<int> Import(string tenant, string environment, string path)
        {
            if (await UploadModel(_apiClient, tenant, environment, path))
                return (int)StatusCodes.Success;
            return (int)StatusCodes.UnknownError;
        }


        private static async Task<bool> UploadModel(IApiClient apiClient, string tenantCode, string environmentCode, string path)
        {
            using var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture))
                {
                    {ReadFileAsStream(path), "file", Path.GetFileName(path)}
                };

            var response = await apiClient.Post($"/api/v1/{tenantCode}/{environmentCode}/model/import", content);
            return response.Success;
        }

        private static async Task<int> BuildModel(IApiClient apiClient, string tenantCode, string environmentCode)
        {

            var requestContent = new StringContent(JsonConvert.SerializeObject(new { Clean = true }), Encoding.UTF8, "application/json");

            var response = await apiClient.Post($"/api/v1/{tenantCode}/{environmentCode}/model/builds", requestContent);
            if (response.Success)
            {
                Console.WriteLine($"Successfully imported and built model to tenant \"{tenantCode}\".");
                return (int)StatusCodes.Success;
            }

            var apiError = response.ErrorDetails;

            Console.WriteLine($"{apiError.Code}: {apiError.Message}");

            return (int)StatusCodes.InvalidOperation;
        }

        private static StreamContent ReadFileAsStream(string path)
            => new StreamContent(new MemoryStream(GetFileAsByteArray(path)));

        private static byte[] GetFileAsByteArray(string path)
            => File.ReadAllBytes(path);

    }
}
