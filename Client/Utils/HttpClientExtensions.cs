using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartProctor.Client.Utils
{
    public static class HttpClientExtensions
    {
        public static async Task<TResponse> PostAsAndGetFromJsonAsync<TRequest, TResponse>(this HttpClient client,
            string? requestUri, TRequest value, JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var res = await client.PostAsJsonAsync(requestUri, value, options, cancellationToken);
            return await res.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}