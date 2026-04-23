using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;
using Frontend.Interfaces;

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
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result ?? new List<Source>();
    }
    public async Task<HttpContent> PostList(List<Source> source)
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/AddList", JsonContent.Create(source));
        return response.Content;
    }

    public async Task Update(Source source)
    {
        HttpResponseMessage response = await _client.PutAsync($"{UrlExtension}/", JsonContent.Create(source));
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id}");
        response.EnsureSuccessStatusCode();
    }
}