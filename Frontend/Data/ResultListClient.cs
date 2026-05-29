using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class ResultListClient : IClient<ResultList>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "ResultList";

    public ResultListClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<ResultList?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResultList>();
    }

    public async Task<List<ResultList>> GetAll()
    {
        var response = await _client.GetAsync(UrlExtension);
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<List<ResultList>>();
        return results ?? [];
    }

    public async Task Post(ResultList resultList)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(resultList));
    }

    public async Task Put(ResultList resultList)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(resultList));
    }

    public async Task Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/{id.ToString()}");
    }
}