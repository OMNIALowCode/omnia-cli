using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    public class DefinitionService
    {
        private readonly IApiClient _apiClient;

        public DefinitionService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task ReplaceBehaviours(string tenant, string environment,
            string entity,
            IList<Data.Behaviour> behaviours)
        {
            var patch = new JsonPatchDocument().Replace("/behaviours", behaviours.ToArray());
            var dataAsString = JsonConvert.SerializeObject(patch);

            var definition = await GetDefinitionForEntity(tenant, environment, entity).ConfigureAwait(false);

            await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
        }

        private async Task<string> GetDefinitionForEntity(string tenant, string environment, string entity)
        {
            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/output/definitions/{entity}")
                .ConfigureAwait(false);

            if (!details.Success) return null;

            var data = content.ReadAsJson<IDictionary<string, string>>();
            if (!data.ContainsKey("instanceOf")) return null;
            return data["instanceOf"]?.ToString();
        }
    }
}