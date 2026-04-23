using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class SourceClient : IClient<Source>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Source";

    public SourceClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Source?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>?> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result;
    }

    public async Task Post(Source source) // changed return type to Task
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(source));
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(Source source) // renamed from Patch
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(source));
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id) // changed return type to Task
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id}");
        response.EnsureSuccessStatusCode();
    }
}