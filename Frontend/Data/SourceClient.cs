using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class SourceClient : BaseClient, IClient<Source>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Source";

    public SourceClient(HttpClient httpClient, PopupHub popupHub): base(httpClient, popupHub)
    {
        _client = httpClient;
    }

    public async Task<Source?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        if (await HandleError(response))
            return null;
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>> GetAll()
    {
        var response = await _client.GetAsync($"{UrlExtension}/");
        if (await HandleError(response))
            return [];
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result ?? [];
    }

    public async Task<bool> Post(Source source)
    {
        var response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(source));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> PostList(List<Source> source)
    {
        var response = await _client.PostAsync($"{UrlExtension}/AddList", JsonContent.Create(source));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Put(Source source)
    {
        var response = await _client.PutAsync($"{UrlExtension}/Update/{source.Id}", JsonContent.Create(source));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id.ToString()}");
        if (await HandleError(response))
            return false;
        return true;
    }
}