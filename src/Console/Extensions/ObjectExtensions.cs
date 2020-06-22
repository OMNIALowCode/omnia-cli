using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Omnia.CLI.Extensions
{
    public static class ObjectExtensions
    {

        public static StringContent ToHttpStringContent(
            this object data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            return new StringContent(dataAsString, Encoding.UTF8, "application/json");
        }
    }
}
