using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.CLI.Infrastructure
{
    public interface IApiClient
    {
        Task<(bool Success, string Content)> Get(string endpoint);
        Task<(bool Success, Stream Content)> GetStream(string endpoint);
        Task<(bool Success, ApiError ErrorDetails)> Patch(string endpoint, HttpContent content);
        Task<(bool Success, ApiError ErrorDetails)> Post(string endpoint, HttpContent content);
        Task<(bool Success, ApiError ErrorDetails)> Delete(string endpoint);
        Task Authenticate(AppSettings.Subscription subscription);
    }
}
