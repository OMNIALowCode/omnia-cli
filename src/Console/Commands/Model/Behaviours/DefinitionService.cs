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

        public async Task<bool> ReplaceBehaviours(string tenant, string environment,
            string entity,
            IList<Data.Behaviour> behaviours)
        {
            var definition = await GetDefinitionForEntity(tenant, environment, entity).ConfigureAwait(false);

            var patch = new JsonPatchDocument().Replace("/entityBehaviours", behaviours.ToArray());
            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }

        private async Task<string> GetDefinitionForEntity(string tenant, string environment, string entity)
        {
            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/output/definitions/{entity}")
                .ConfigureAwait(false);

            if (!details.Success) return null;

            var data = ((Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(content));

            if (data.TryGetValue("instanceOf", out var definition))
                return definition.ToString();
            return null;
        }
    }
}