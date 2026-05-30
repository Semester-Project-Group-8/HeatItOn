using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class OptimizedResultsClient : BaseClient,
    IClient<OptimizedResults>
{
    private readonly HttpClient _client;
    private const string urlExtension = "OptimizedResults";

    public OptimizedResultsClient(HttpClient httpClient, PopupHub popupHub) : base(httpClient, popupHub)
    {
        _client = httpClient;
    }

    public async Task<OptimizedResults?> Get(int id)
    {
        var response = await _client.GetAsync($"{urlExtension}/{id.ToString()}");
        
        if (await HandleError(response))
            return null;
        
        return await response.Content.ReadFromJsonAsync<OptimizedResults>();
    }

    public async Task<List<OptimizedResults>?> GetAll()
    {
        var response = await _client.GetAsync($"{urlExtension}");
        if (await HandleError(response))
            return [];
        var result = await response.Content.ReadFromJsonAsync<List<OptimizedResults>>();
        return result;
    }

    public async Task<bool> Post(OptimizedResults item)
    {
        var response = await _client.PostAsJsonAsync($"{urlExtension}", item);
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Put(OptimizedResults item)
    {
        var response = await _client.PutAsJsonAsync($"{urlExtension}", item);
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var response = await _client.DeleteAsync($"{urlExtension}/Delete/{id.ToString()}");
        if (await HandleError(response))
            return false;
        return true;
    }
}