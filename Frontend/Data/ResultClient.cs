using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class ResultClient : IClient<Result>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Result";

    public ResultClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Result?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Result>();
    }

    public async Task<List<Result>> GetAll()
    {
        var response = await _client.GetAsync($"{UrlExtension}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Result>>() ?? [];
    }

    public async Task Post(Result result)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(result));
    }

    public async Task Put(Result result)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(result));
    }

    public async Task Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/{id.ToString()}");
    }
}