using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class OptimizerClient: BaseClient
{
    private readonly HttpClient _client;

    public OptimizerClient(HttpClient httpClient, PopupHub popupHub):base(httpClient, popupHub)
    {
        _client = httpClient;
    }

    public async Task<HttpResponseMessage> Optimize(List<Asset> scenarioAssets)
    {
        var result = await _client.PostAsync("Optimize", JsonContent.Create(scenarioAssets));

        await HandleError(result);
        
        return result;
    }
}