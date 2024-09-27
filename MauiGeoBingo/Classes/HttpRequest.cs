using System.Net;
using System.Text;
using System.Text.Json;

namespace MauiGeoBingo.Classes;

internal class HttpRequest
{
    private Uri _uri;
    private HttpClient _httpClient;

    public HttpRequest(string uri)
    {
        _uri = new Uri(uri);
        _httpClient = new HttpClient();
    }

    public async Task<T?> GetAsync<T>()
    {
        var response = await _httpClient.GetAsync(_uri);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse);
        }
        return default;
    }

    public async Task<T?> PostAsync<T>(object? args)
    {
        StringContent jsonContent = new(JsonSerializer.Serialize(args), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.PostAsync(_uri, jsonContent);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse);
        }
        return default;
    }

    public async Task<T?> PutAsync<T>(object? args)
    {
        StringContent jsonContent = new(JsonSerializer.Serialize(args), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.PutAsync(_uri, jsonContent);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse);
        }
        return default;
    }

    public async Task<T?> DeleteAsync<T>()
    {
        using HttpResponseMessage response = await _httpClient.DeleteAsync(_uri);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse);
        }
        return default;
    }
}




