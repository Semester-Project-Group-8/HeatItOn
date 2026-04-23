using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

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

    public async Task<List<Result>?> GetAll() // added ? to match interface
    {
        HttpResponseMessage response = await _client.GetAsync("");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Result>>();
        return result;
    }

    public async Task Post(Result result) // changed return type to Task
    {
        HttpResponseMessage response = await _client.PostAsync("", JsonContent.Create(result));
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(Result result) // renamed from Patch
    {
        HttpResponseMessage response = await _client.PostAsync("", JsonContent.Create(result));
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete(int id) // changed return type to Task
    {
        HttpResponseMessage response = await _client.DeleteAsync(id.ToString());
        response.EnsureSuccessStatusCode();
    }
}