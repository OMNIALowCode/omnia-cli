using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Omnia.CLI.Commands.Model.Apply.Data.Server;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class ApplicationBehaviourApplyService
    {
        private readonly IApiClient _apiClient;

        public ApplicationBehaviourApplyService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> ReplaceData(string tenant, string environment,
                    string entity,
                    ApplicationBehaviour applicationBehaviourData)
        {
            var patch = new JsonPatchDocument();

            patch.Replace("/expression", applicationBehaviourData.Expression);

            if (applicationBehaviourData.Usings?.Count > 0)
                patch.Replace("/behaviourNamespaces",
                    applicationBehaviourData.Usings.Select(u => MapToBehaviourNamespace(u, applicationBehaviourData.Namespace)));

            var dataAsString = JsonConvert.SerializeObject(patch);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/ApplicationBehaviour/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }

        private static object MapToBehaviourNamespace(string usingDirective, string @namespace)
            => new
            {
                Name = usingDirective.Replace(".", ""),
                ExecutionLocation = GetLocationFromNamespace(@namespace),
                FullyQualifiedName = usingDirective
            };

        private static string GetLocationFromNamespace(string @namespace)
            => @namespace.Split('.').ElementAtOrDefault(3) ?? "Internal";
    }
}