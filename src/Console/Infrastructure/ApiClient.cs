using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    public interface IApiClient
    {
        Task<(bool Success, string Content)> Get(string endpoint);
        Task<(bool Success, ApiError ErrorDetails)> Patch(string endpoint, HttpContent content);
        Task<(bool Success, ApiError ErrorDetails)> Post(string endpoint, HttpContent content);
        Task Authenticate(AppSettings.Subscription subscription);
    }

    public class ApiClient : IApiClient
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        public ApiClient(HttpClient httpClient, IAuthenticationProvider authenticationProvider)
        {
            HttpClient = httpClient;
            _authenticationProvider = authenticationProvider;
        }

        public HttpClient HttpClient { get; }

        public async Task<(bool Success, string Content)> Get(string endpoint)
        {
            var response = await HttpClient.GetAsync(endpoint);

            var responseMessage = await response.Content.ReadAsStringAsync();

            return (response.IsSuccessStatusCode, responseMessage);
        }

        public async Task<(bool Success, ApiError ErrorDetails)> Patch(string endpoint, HttpContent content)
        {
            //TODO: Send ETAG
            using var response = await HttpClient.PatchAsync(endpoint,
                content);

            return response.IsSuccessStatusCode ? (true, null) : (false, await GetErrorFromApiResponse(response));
        }

        public async  Task<(bool Success, ApiError ErrorDetails)> Post(string endpoint, HttpContent content)
        {
            using var response = await HttpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode ? (true, null) : (false, await GetErrorFromApiResponse(response));
        }


        public async Task Authenticate(AppSettings.Subscription subscription)
        {
            await _authenticationProvider.AuthenticateClient(HttpClient, subscription);
        }

        private static async Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
    }
}
