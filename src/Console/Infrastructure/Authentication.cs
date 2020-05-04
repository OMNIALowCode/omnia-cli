using IdentityModel.Client;
using System;
using System.Net.Http;

namespace Omnia.CLI.Infrastructure
{
    internal class Authentication
    {
        private readonly Uri _identityUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public Authentication(Uri identityUrl, string clientId, string clientSecret)
        {
            _identityUrl = identityUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async System.Threading.Tasks.Task<string> AuthenticateAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(_identityUrl.ToString());

            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
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
