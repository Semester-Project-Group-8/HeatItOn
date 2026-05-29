using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class OptimizedResultsClient : IClient<OptimizedResults>
{
    private readonly HttpClient _client;
    private const string urlExtension = "OptimizedResults";

    public OptimizedResultsClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<OptimizedResults?> Get(int id)
    {
        var response = await _client.GetAsync($"{urlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OptimizedResults>();
    }

    public async Task<List<OptimizedResults>?> GetAll()
    {
        var response = await _client.GetAsync($"{urlExtension}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<OptimizedResults>>();
        return result;
    }

    public async Task Post(OptimizedResults item)
    {
        var response = await _client.PostAsJsonAsync($"{urlExtension}", item);
    }

    public async Task Put(OptimizedResults item)
    {
        var response = await _client.PutAsJsonAsync($"{urlExtension}", item);
    }

    public async Task Delete(int id)
    {
        var response = await _client.DeleteAsync($"{urlExtension}/Delete/{id.ToString()}");
    }
}