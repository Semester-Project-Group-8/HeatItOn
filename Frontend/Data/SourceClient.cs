using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class SourceClient
{
    private readonly HttpClient _httpClient;

    public SourceClient()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8080/Source")
        };
    }

    public async Task<Source?> Get(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(id.ToString());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>?> GetAll()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result;
    }

    public async Task<string> Post(Source source)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(source));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Source?> Patch(Source source)
    {
        HttpResponseMessage response = await _httpClient.PostAsync("", JsonContent.Create(source));
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<Source?> Delete<T>(int id)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(id.ToString() );
        return await response.Content.ReadFromJsonAsync<Source>();
    }
}