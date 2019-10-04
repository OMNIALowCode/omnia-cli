using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

 namespace Omnia.CLI.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString, Encoding.UTF8, "application/json");
            return httpClient.PostAsync(url, content);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync(
            this HttpClient httpClient, string url, JsonPatchDocument data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString, Encoding.UTF8, "application/json");
            return httpClient.PatchAsync(url, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
    }
}
