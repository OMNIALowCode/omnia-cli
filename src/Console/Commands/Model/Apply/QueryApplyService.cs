using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Omnia.CLI.Commands.Model.Apply.Data.Database;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class QueryApplyService
    {
        private readonly IApiClient _apiClient;
        
        public QueryApplyService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> ReplaceData(string tenant, string environment,
                    string entity,
                    Query data)
        {
            var patch = new JsonPatchDocument()
                .Replace("/expression", data.Expression);

            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/query/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }
    }
}