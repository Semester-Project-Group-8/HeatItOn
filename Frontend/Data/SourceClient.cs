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
        var response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>> GetAll()
    {
        var response = await _client.GetAsync($"{UrlExtension}/");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result ?? [];
    }

    public async Task Post(Source source)
    {
        var response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(source));
    }

    public async Task PostList(List<Source> source)
    {
        var response = await _client.PostAsync($"{UrlExtension}/AddList", JsonContent.Create(source));
    }

    public async Task Put(Source source)
    {
        var response = await _client.PutAsync($"{UrlExtension}/Update/{source.Id}", JsonContent.Create(source));
    }

    public async Task Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id.ToString()}");
    }
}