using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class AssetClient : BaseClient, IClient<Asset>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Asset";

    public AssetClient(HttpClient httpClient, PopupHub popupHub) : base(httpClient,  popupHub)
    {
        _client = httpClient;
    }

    public async Task<Asset?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id}");

        if (await HandleError(response))
            return null;
        
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    public async Task<List<Asset>> GetAll()
    {
        var response = await _client.GetAsync($"{UrlExtension}");
        
        if (await HandleError(response))
            return [];

        var result = await response.Content.ReadFromJsonAsync<List<Asset>>();
        return result ?? [];
    }

    public async Task<bool> Post(Asset asset)
    {
        var response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(asset));
        await HandleError(response);
        
        if (await HandleError(response)) return false;
        return true;
    }

    public async Task<bool> Put(Asset asset)
    {
        var response = await _client.PutAsync($"{UrlExtension}/{asset.Id}", JsonContent.Create(asset));
        
        if (await HandleError(response)) return false;
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/{id}");
        
        if (await HandleError(response)) return false;
        return true;
    }
}