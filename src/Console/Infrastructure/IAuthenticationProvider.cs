using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    public interface IAuthenticationProvider
    {
        Task<HttpClient> AuthenticateClient(HttpClient httpClient, AppSettings.Subscription subscription);
    }
}
