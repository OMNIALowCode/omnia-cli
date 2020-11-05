using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    public interface IApiClient
    {
        Task<(ApiResponse ApiDetails, string Content)> Get(string endpoint);

        Task<(ApiResponse ApiDetails, Stream Content)> GetStream(string endpoint);

        Task<ApiResponse> Patch(string endpoint, HttpContent content);

        Task<ApiResponse> Post(string endpoint, HttpContent content);

        Task<ApiResponse> Delete(string endpoint);

        Task Authenticate(AppSettings.Subscription subscription);
    }
}
