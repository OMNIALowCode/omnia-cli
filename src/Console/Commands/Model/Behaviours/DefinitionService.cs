using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Omnia.CLI.Commands.Model.Behaviours.Data;
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

        public async Task<bool> ReplaceApplicationBehaviourData(string tenant, string environment,
            string entity,
            ApplicationBehaviour applicationBehaviourData)
        {
            var patch = new JsonPatchDocument();

            patch.Replace("/expression", applicationBehaviourData.Expression);

            var dataAsString = JsonConvert.SerializeObject(patch, new Newtonsoft.Json.Converters.StringEnumConverter());

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/ApplicationBehaviour/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }

        public async Task<bool> ReplaceEntityData(string tenant, string environment,
            string entity,
            Entity entityData)
        {
            var definition = await GetDefinitionForEntity(tenant, environment, entity).ConfigureAwait(false);

            var patch = new JsonPatchDocument();
            if (entityData.EntityBehaviours?.Count > 0)
                patch.Replace("/entityBehaviours", entityData.EntityBehaviours.ToArray());
            if (entityData.DataBehaviours?.Count > 0)
                patch.Replace("/dataBehaviours", entityData.DataBehaviours.ToArray());
            if (entityData.Usings?.Count > 0)
                patch.Replace("/behaviourNamespaces",
                    entityData.Usings.Select(u => MapToBehaviourNamespace(u, entityData.Namespace)));

            var dataAsString = JsonConvert.SerializeObject(patch, new Newtonsoft.Json.Converters.StringEnumConverter());

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

        private static object MapToBehaviourNamespace(string usingDirective, string @namespace)
        => new
        {
            Name = usingDirective.Replace(".", ""),
            ExecutionLocation = @namespace.Split('.').ElementAtOrDefault(3) ?? "Internal",
            FullyQualifiedName = usingDirective
        };

    }
}