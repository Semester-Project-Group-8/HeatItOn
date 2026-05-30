using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class ResultListClient : BaseClient, IClient<ResultList>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "ResultList";

    public ResultListClient(HttpClient httpClient, PopupHub popupHub) : base(httpClient, popupHub)
    {
        _client = httpClient;
    }

    public async Task<ResultList?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id}");
        if (await HandleError(response))
            return null;
        return await response.Content.ReadFromJsonAsync<ResultList>();
    }

    public async Task<List<ResultList>> GetAll()
    {
        var response = await _client.GetAsync(UrlExtension);
        if (await HandleError(response))
            return [];
        var results = await response.Content.ReadFromJsonAsync<List<ResultList>>();
        return results ?? [];
    }

    public async Task<bool> Post(ResultList resultList)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(resultList));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Put(ResultList resultList)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(resultList));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var response = await _client.DeleteAsync($"{UrlExtension}/{id.ToString()}");
        if (await HandleError(response))
            return false;
        return true;
    }
}