using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;
using Omnia.CLI.Commands.Model.Extensions;

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

            return await _apiClient.BuildModel(tenant, environment);
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

        private static StreamContent ReadFileAsStream(string path)
            => new StreamContent(new MemoryStream(GetFileAsByteArray(path)));

        private static byte[] GetFileAsByteArray(string path)
            => File.ReadAllBytes(path);

    }
}
