using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Clinica.API.IntegrationTests.Helpers;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void WithBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static void WithoutAuthorization(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public static async Task<T?> ReadDataAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var data = await JsonTestHelper.ReadDataAsync(response);

        return data.Deserialize<T>(JsonOptions);
    }

    public static async Task<string> ReadContentAsStringAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<HttpResponseMessage> PostJsonAsync<T>(
        this HttpClient client,
        string url,
        T body)
    {
        return await client.PostAsJsonAsync(url, body, JsonOptions);
    }

    public static async Task<HttpResponseMessage> PutJsonAsync<T>(
        this HttpClient client,
        string url,
        T body)
    {
        return await client.PutAsJsonAsync(url, body, JsonOptions);
    }
    
    public static void SetBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearBearerToken(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
    
}