using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;
using Frontend.Interfaces;

namespace Frontend.Data;

public class ResultListClient : IClient<ResultList>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "ResultList";

    public ResultListClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<List<ResultList>> GetAll()
    {
        var response = await _client.GetAsync(UrlExtension);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ResultList>>();
        return result ?? new List<ResultList>();
    }

    public async Task<ResultList?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResultList>();
    }

    public async Task Post(ResultList item)
    {
        var response = await _client.PostAsJsonAsync(UrlExtension, item);
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(ResultList item)
    {
        var response = await _client.PutAsJsonAsync(UrlExtension, item);
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
    }
}