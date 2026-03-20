using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class AssetClient
{
    
    private readonly HttpClient _client;
    private const string urlExtension = "Asset";

    public AssetClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Asset?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{urlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<List<Asset>?> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{urlExtension}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Asset>>();
        return result;
    }

    public async Task<string> Post(Asset asset)
    {
        HttpResponseMessage response = await _client.PostAsync($"{urlExtension}/Add", JsonContent.Create(asset));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Asset?> Patch(Asset asset)
    {
        HttpResponseMessage response = await _client.PostAsync($"{urlExtension}/{asset.Id}", JsonContent.Create(asset));
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<Asset?> Delete<T>(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{urlExtension}/{id.ToString()}");
        return await response.Content.ReadFromJsonAsync<Asset>();
    }
}