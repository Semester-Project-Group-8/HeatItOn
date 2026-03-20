using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Data;

public class OptimizerClient
{
    private readonly HttpClient _httpClient;
    
    public OptimizerClient(HttpClient httpClient)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8080/Optimizer")
        };
    }

    public async Task<HttpResponseMessage> Optimize(List<int> assetIds)
    {
        var result = await _httpClient.PostAsync("Optimize", JsonContent.Create(assetIds));
        return result;
    }
}