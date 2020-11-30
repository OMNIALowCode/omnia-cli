using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.Commands.Model.Apply
{
    public class ThemeApplyService
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

        public ThemeApplyService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> ReplaceData(string tenant, string environment,
                    string entity,
                    Theme data)
        {
            var patch = new JsonPatchDocument()
                .Replace("/expression", data.Expression);

            var dataAsString = JsonConvert.SerializeObject(patch, SerializeSettings);

            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/theme/{entity}",
                new StringContent(dataAsString,
                                Encoding.UTF8,
                                "application/json")).ConfigureAwait(false);
            return response.Success;
        }
    }
}