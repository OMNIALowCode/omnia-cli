using Omnia.CLI.Extensions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationProvider _authenticationProvider;
        public ApiClient(HttpClient httpClient, IAuthenticationProvider authenticationProvider)
        {
            _httpClient = httpClient;
            _authenticationProvider = authenticationProvider;
        }

        public async Task<(bool Success, string Content)> Get(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            var responseMessage = await response.Content.ReadAsStringAsync();

            return (response.IsSuccessStatusCode, responseMessage);
        }

        public async Task<(bool Success, Stream Content)> GetStream(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            var responseMessage = await response.Content.ReadAsStreamAsync();

            return (response.IsSuccessStatusCode, responseMessage);
        }

        public async Task<(bool Success, ApiError ErrorDetails)> Patch(string endpoint, HttpContent content)
        {
            //TODO: Send ETAG
            using var response = await _httpClient.PatchAsync(endpoint,
                content);

            return response.IsSuccessStatusCode ? (true, null) : (false, await GetErrorFromApiResponse(response));
        }

        public async Task<(bool Success, ApiError ErrorDetails)> Post(string endpoint, HttpContent content)
        {
            using var response = await _httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode ? (true, null) : (false, await GetErrorFromApiResponse(response));
        }

        public async Task<(bool Success, ApiError ErrorDetails)> Delete(string endpoint)
        {
            using var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode ? (true, null) : (false, await GetErrorFromApiResponse(response));
        }


        public async Task Authenticate(AppSettings.Subscription subscription)
        {
            await _authenticationProvider.AuthenticateClient(_httpClient, subscription);
        }

        private static async Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => await response.Content.ReadAsJsonAsync<ApiError>() ?? new ApiError()
            {
                Code = ((int)response.StatusCode).ToString(),
                Message = (int)response.StatusCode != 403 ? response.StatusCode.ToString() : "Access denied!"
            };
    }
}
