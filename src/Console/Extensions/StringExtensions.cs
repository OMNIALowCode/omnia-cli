using Newtonsoft.Json;

namespace Omnia.CLI.Extensions
{
    public static class StringExtensions
    {
        public static T ReadAsJson<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
