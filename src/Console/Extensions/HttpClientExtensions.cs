using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString, Encoding.UTF8, "application/json");
            return httpClient.PostAsync(url, content);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync(
            this HttpClient httpClient, string url, JsonPatchDocument data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString, Encoding.UTF8, "application/json");
            return httpClient.PatchAsync(url, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }

        public static async Task WithSubscription(this HttpClient httpClient, AppSettings.Subscription subscription)
        {
            var authentication = new Authentication(subscription.IdentityServerUrl,
                subscription.Client.Id,
                subscription.Client.Secret);

            var accessToken = await authentication.AuthenticateAsync();
            var authValue = new AuthenticationHeaderValue("Bearer", accessToken);

            httpClient.DefaultRequestHeaders.Authorization = authValue;
            httpClient.BaseAddress = subscription.ApiUrl;
        }
    }
}
