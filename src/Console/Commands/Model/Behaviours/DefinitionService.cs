using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
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
            string definition, string entity,
            IList<Data.Behaviour> behaviours)
        {


            var patch = new JsonPatchDocument().Replace("/behaviours", behaviours.ToArray());
            var dataAsString = JsonConvert.SerializeObject(patch);

            await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
                new StringContent(dataAsString,
                                  Encoding.UTF8,
                                  "application/json"));

        }

    }
}