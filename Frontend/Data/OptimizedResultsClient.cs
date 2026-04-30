using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;
using Frontend.Interfaces;

namespace Frontend.Data;

public class OptimizedResultsClient : IClient<OptimizedResults>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "OptimizedResults";

    public OptimizedResultsClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<OptimizedResults?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OptimizedResults>();
    }

    public async Task<List<OptimizedResults>> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<OptimizedResults>>();
        return result ?? new List<OptimizedResults>();
    }

    public async Task Post(OptimizedResults item)
    {
        var response = await _client.PostAsJsonAsync(UrlExtension, item);
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(OptimizedResults item)
    {
        var response = await _client.PutAsJsonAsync(UrlExtension, item);
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id}");
        response.EnsureSuccessStatusCode();
    }
}