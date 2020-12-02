using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class UIBehavioursApplyService
    {
        private readonly IApiClient _apiClient;
        public UIBehavioursApplyService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> ReplaceData(string tenant, string environment,
                    string entity,
                    UIEntity data)
        {
            var definition = await GetDefinitionForEntity(tenant, environment, entity).ConfigureAwait(false);
            var patch = new JsonPatchDocument()
                .Replace("/behaviours", data.EntityBehaviours.ToArray());

            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }


        private async Task<string> GetDefinitionForEntity(string tenant, string environment, string entity)
        {
            if(entity.Equals("DefaultApplicationMenu")) return "ApplicationMenu";

            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/output/metadata/{entity}")
                .ConfigureAwait(false);

            if (!details.Success) return null;

            var data = ((JObject)JsonConvert.DeserializeObject(content));

            if (data.TryGetValue("type", out var definition))
                return definition.ToString();
            return null;
        }
    }
}