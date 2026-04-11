using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class SourceClient
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Source";

    public SourceClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<Source?> Get(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<List<Source>?> GetAll()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Source>>();
        return result;
    }

    public async Task<string> Post(Source source)
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/Add", JsonContent.Create(source));
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Source?> Patch(Source source)
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(source));
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task<Source?> Delete(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"{UrlExtension}/Delete/{id.ToString()}" );
        return await response.Content.ReadFromJsonAsync<Source>();
    }

    public async Task ImportCsv()
    {
        HttpResponseMessage response = await _client.PostAsync($"{UrlExtension}/ImportCsv", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> ExportCsv()
    {
        HttpResponseMessage response = await _client.GetAsync($"{UrlExtension}/ExportCsv");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return (result);
    }

}