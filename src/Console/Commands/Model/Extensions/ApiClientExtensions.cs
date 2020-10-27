using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Extensions
{
    public static class ApiClientExtensions
    {
        public static async Task<int> BuildModel(this IApiClient apiClient, string tenantCode, string environmentCode)
        {
            var requestContent = new StringContent(JsonConvert.SerializeObject(new { Clean = true }), Encoding.UTF8, "application/json");

            var response = await apiClient.Post($"/api/v1/{tenantCode}/{environmentCode}/model/builds", requestContent)
                .ConfigureAwait(false);

            if (response.Success)
            {
                Console.WriteLine($"Successfully imported and built model to tenant \"{tenantCode}\".");
                return (int)StatusCodes.Success;
            }

            var apiError = response.ErrorDetails;

            Console.WriteLine($"{apiError.Code}: {apiError.Message}");

            return (int)StatusCodes.InvalidOperation;
        }
    }
}