using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Models;

namespace Frontend.Data;

public class AssetClient
{
    
    private readonly HttpClient _httpClient;

    public AssetClient()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8080/Asset")
        };
    }

    public async Task<Asset?> Get(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(id.ToString());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<List<Asset>?> GetAll()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Asset>>();
        return result;
    }

    public async Task<string> Post(Asset asset)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(asset));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Asset?> Patch(Asset asset)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(asset));
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<Asset?> Delete<T>(int id)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(id.ToString() );
        return await response.Content.ReadFromJsonAsync<Asset>();
    }
}