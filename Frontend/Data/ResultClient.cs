using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Data;

public class ResultClient : BaseClient, IClient<Result>
{
    private readonly HttpClient _client;
    private const string UrlExtension = "Result";

    public ResultClient(HttpClient httpClient, PopupHub popupHub) : base(httpClient, popupHub)
    {
        _client = httpClient;
    }

    public async Task<Result?> Get(int id)
    {
        var response = await _client.GetAsync($"{UrlExtension}/{id.ToString()}");
        if (await HandleError(response))
            return null;
        return await response.Content.ReadFromJsonAsync<Result>();
    }

    public async Task<List<Result>> GetAll()
    {
        var response = await _client.GetAsync($"{UrlExtension}");
        if (await HandleError(response))
            return [];
        return await response.Content.ReadFromJsonAsync<List<Result>>() ?? [];
    }

    public async Task<bool> Post(Result result)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(result));
        if (await HandleError(response))
            return false;
        return true;
    }

    public async Task<bool> Put(Result result)
    {
        var response = await _client.PostAsync($"{UrlExtension}/", JsonContent.Create(result));
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