using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
    public async Task<List<ResultList>?> GetAll() // renamed from ListResultLists
    {
        var response = await _client.GetAsync(UrlExtension);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ResultList>>();
    }
    
    public async Task<ResultList?> Get(int id) // renamed from GetResultListById
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResultList>();
    }

    public Task Post(ResultList item) => throw new NotImplementedException();
    public Task Update(ResultList item) => throw new NotImplementedException();
    public Task Delete(int id) => throw new NotImplementedException();
}