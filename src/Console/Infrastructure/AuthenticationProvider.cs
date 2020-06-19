using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    internal class AuthenticationProvider : IAuthenticationProvider
    {
        public async Task<HttpClient> AuthenticateClient(HttpClient httpClient, AppSettings.Subscription subscription)
        {
            var accessToken = await AuthenticateAsync(httpClient, subscription);
            var authValue = new AuthenticationHeaderValue("Bearer", accessToken);

            httpClient.DefaultRequestHeaders.Authorization = authValue;
            httpClient.BaseAddress = subscription.ApiUrl;
            
            return httpClient;
        }

        private static async Task<string> AuthenticateAsync(HttpClient httpClient, AppSettings.Subscription subscription)
        {

            var disco = await httpClient.GetDiscoveryDocumentAsync(subscription.IdentityServerUrl.ToString());

            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            // request token
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = subscription.Client.Id,
                ClientSecret = subscription.Client.Secret,
                Scope = "api"
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }
            return tokenResponse.AccessToken;
        }
    }
}
