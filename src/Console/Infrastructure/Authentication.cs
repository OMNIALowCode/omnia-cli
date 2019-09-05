using IdentityModel.Client;
using System;

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
            var identityClient = new DiscoveryClient(_identityUrl.ToString());
            var disco = await identityClient.GetAsync();
            if (disco.IsError)
            {
                throw new Exception(disco.Error);

            }

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, _clientId, _clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api");

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }
            return tokenResponse.AccessToken;
        }
    }
}
