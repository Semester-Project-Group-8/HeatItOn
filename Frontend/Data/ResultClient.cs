using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;
using Frontend.Interfaces;

namespace Frontend.Data;

public class ResultClient : IClient<Result>
{
    private readonly HttpClient _client;

    public ResultClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Result?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync(id.ToString());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Result>();
    }

    public async Task<List<Result>?> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync("");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Result>>();
        return result;
    }

    public async Task Post(Result result)
    {
        HttpResponseMessage response = await _client.PostAsync("", JsonContent.Create(result));
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(Result result)
    {
        HttpResponseMessage response = await _client.PostAsync("", JsonContent.Create(result));
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync(id.ToString());
        response.EnsureSuccessStatusCode();
    }
}