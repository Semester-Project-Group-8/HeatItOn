using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Models;

namespace Frontend.Data;

public class ResultClient
{
    private readonly HttpClient _httpClient;

    public ResultClient()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8080/Result")
        };
    }

    public async Task<Result?> Get(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(id.ToString());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Result>();
    }

    public async Task<List<Result>?> GetAll()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Result>>();
        return result;
    }

    public async Task<string> Post(Result result)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(result));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Result?> Patch(Result result)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(result));
        return await response.Content.ReadFromJsonAsync<Result>();
    }

    public async Task<Result?> Delete<T>(int id)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(id.ToString() );
        return await response.Content.ReadFromJsonAsync<Result>();
    }
}