using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace santa_shares
{
    public static class HttpExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
           this HttpClient httpClient, string url, T data, AuthenticationHeaderValue auth = null)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            Console.WriteLine(dataAsString);
            if (!(auth is null)) httpClient.DefaultRequestHeaders.Authorization = auth;
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpClient.PostAsync(url, content);
            
        }

        public static Task<HttpResponseMessage> GetAsJsonAsync<T>(
           this HttpClient httpClient, string url, AuthenticationHeaderValue auth = null)
        {
            if (!(auth is null)) httpClient.DefaultRequestHeaders.Authorization = auth;
            return httpClient.GetAsync(url);
        }

        public static async Task<T> GetTypeAsJsonAsync<T>(
           this HttpClient httpClient, string url, AuthenticationHeaderValue auth = null)
        {
            if (!(auth is null)) httpClient.DefaultRequestHeaders.Authorization = auth;
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);
            T response = await httpResponseMessage.Content.ReadAsJsonAsync<T>();
            return response;
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            Console.WriteLine(dataAsString);
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
    }
}