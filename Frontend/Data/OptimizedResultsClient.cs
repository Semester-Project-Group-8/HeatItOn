using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class OptimizedResultsClient
{
    private readonly HttpClient _client;
    private const string urlExtension = "OptimizedResults";

    public OptimizedResultsClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<OptimizedResults?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{urlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OptimizedResults>();
    }

    public async Task<List<OptimizedResults>?> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{urlExtension}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<OptimizedResults>>();
        return result;
    }

    public async void Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{urlExtension}/Delete/{id.ToString()}");
    }
}