using Omnia.CLI.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        public async Task<(ApiResponse, string)> Get(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            var responseMessage = await response.Content.ReadAsStringAsync();

            return (new ApiResponse(response.IsSuccessStatusCode, response.StatusCode, new Dictionary<string, object> {
                { "ETAG", response.Headers.ETag }}
            ), responseMessage);
        }

        public async Task<(ApiResponse, Stream)> GetStream(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            var responseMessage = await response.Content.ReadAsStreamAsync();

            return (new ApiResponse(response.IsSuccessStatusCode, response.StatusCode, new Dictionary<string, object> {
                { "ETAG", response.Headers.ETag }}
            ), responseMessage);
        }

        public async Task<ApiResponse> Patch(string endpoint, HttpContent content)
        {
            //TODO: Send ETAG
            var response = await _httpClient.PatchAsync(endpoint,
                content);

            return response.IsSuccessStatusCode ? new ApiResponse(true, response.StatusCode, errorDetails: null) : new ApiResponse(false, response.StatusCode, await GetErrorFromApiResponse(response));
        }

        public async Task<ApiResponse> Post(string endpoint, HttpContent content)
        {
            var response = await _httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode ? new ApiResponse(true, response.StatusCode, errorDetails: null) : new ApiResponse(false, response.StatusCode, await GetErrorFromApiResponse(response));
        }

        public async Task<ApiResponse> Delete(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode ? new ApiResponse(true, response.StatusCode, errorDetails: null) : new ApiResponse(false, response.StatusCode, await GetErrorFromApiResponse(response));
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
