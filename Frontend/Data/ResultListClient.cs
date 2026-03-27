using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class ResultListClient
{
    private readonly HttpClient _client;
    
    public ResultListClient(HttpClient httpClient)
    {
        _client = httpClient;
    }
    public async Task<List<int>?> ListResultLists()
    {
        var response = await _client.GetAsync("");
        return await response.Content.ReadFromJsonAsync<List<int>>();
    }
    
    public async Task<ResultList?> GetResultListById(int id)
    {
        var response = await _client.GetAsync($"{id}");
        return await response.Content.ReadFromJsonAsync<ResultList>();
    }
}