using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Omnia.CLI.Commands.Model.Apply.Data;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class UIBehavioursApplyService
    {
        private readonly IApiClient _apiClient;
        private static readonly JsonSerializerSettings SerializeSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
        };

        public UIBehavioursApplyService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> ReplaceData(string tenant, string environment,
                    string entity,
                    UIEntity data)
        {
            var definition = await GetMetadataForEntity(tenant, environment, entity).ConfigureAwait(false);
            var patch = new JsonPatchDocument()
                .Replace("/behaviours", data.EntityBehaviours.ToArray());

            var dataAsString = JsonConvert.SerializeObject(patch, SerializeSettings);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }


        private async Task<string> GetMetadataForEntity(string tenant, string environment, string entity)
        {
            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/output/metadata/{entity}")
                .ConfigureAwait(false);

            if (!details.Success) return null;

            var data = ((JObject)JsonConvert.DeserializeObject(content, SerializeSettings));

            if (data.TryGetValue("type", out var definition))
                return definition.ToString();
            return null;
        }
    }
}