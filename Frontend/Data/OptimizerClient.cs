using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Data;

public class OptimizerClient
{
    private readonly HttpClient _client;
    
    public OptimizerClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<HttpResponseMessage> Optimize(List<int> assetIds)
    {
        var result = await _client.PostAsync("Optimize", JsonContent.Create(assetIds));
        return result;
    }
}