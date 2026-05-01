using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;
using Frontend.Interfaces;

namespace Frontend.Data;

public class AssetClient : IClient<Asset>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Asset";

    public AssetClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Asset?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<List<Asset>> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Asset>>();
        return result ?? new List<Asset>();
    }

    public async Task Post(Asset asset)
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(asset));
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(Asset asset)
    {
        HttpResponseMessage response = await _client.PutAsync($"{UrlExtension}/{asset.Id}", JsonContent.Create(asset));
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
    }
}