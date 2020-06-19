using System.Net.Http;
using System.Threading.Tasks;
using Omnia.CLI;
using Omnia.CLI.Infrastructure;

namespace UnitTests.Fakes
{
    internal class FakeAuthenticationProvider : IAuthenticationProvider
    {
        public Task<HttpClient> AuthenticateClient(HttpClient httpClient, AppSettings.Subscription subscription)
            => Task.FromResult(httpClient);
        
    }
}
