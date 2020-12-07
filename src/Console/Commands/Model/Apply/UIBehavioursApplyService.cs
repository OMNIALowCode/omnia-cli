using System.Collections.Generic;
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

            var patch = new JsonPatchDocument();

            var entityBehaviours = data.EntityBehaviours.Where(a => !string.IsNullOrEmpty(a.Element)).ToList();
            if (entityBehaviours.Count() > 0)
            {
                var metadata = await GetMetadataForForm(tenant, environment, entity).ConfigureAwait(false);

                patch = PatchAttributesToReplace(patch, metadata, entityBehaviours);
            }

            patch.Replace("/behaviours", data.EntityBehaviours.Except(entityBehaviours).ToArray());

            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }


        private async Task<JObject> GetMetadataForForm(string tenant, string environment, string entity)
        {
            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/Form/{entity}")
        .ConfigureAwait(false);

            if (!details.Success) return null;

            return (JObject)JsonConvert.DeserializeObject(content);
        }

        private async Task<string> GetDefinitionForEntity(string tenant, string environment, string entity)
        {
            if (entity.Equals("DefaultApplicationMenu")) return "ApplicationMenu";

            var (details, content) = await _apiClient.Get($"/api/v1/{tenant}/{environment}/model/output/metadata/{entity}")
                .ConfigureAwait(false);

            if (!details.Success) return null;

            var data = ((JObject)JsonConvert.DeserializeObject(content));

            if (data.TryGetValue("type", out var definition))
                return definition.ToString();
            return null;
        }

        private static JsonPatchDocument PatchAttributesToReplace(JsonPatchDocument patch, JObject metadata, IList<UIBehaviour> behaviours)
        {
            if (!metadata.TryGetValue("elements", out var metadataElements)) return null;

            for (var sn = 0; sn < metadataElements.Count(); sn++)
            {
                var element = behaviours.FirstOrDefault(s => s.Element.Equals(metadataElements[sn]["name"].Value<string>()));
                if (element == null) continue;

                patch.Replace($"/elements/{sn}/behaviours", new UIBehaviour[] { element });
            }
            return patch;
        }
    }
}