using Newtonsoft.Json;

namespace Omnia.CLI.Extensions
{
    public static class StringExtensions
    {
        public static T ReadAsJson<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            var result = target;
            if (result.StartsWith(trimString))
                result = result[trimString.Length..];

            return result;
        }
    }
}
