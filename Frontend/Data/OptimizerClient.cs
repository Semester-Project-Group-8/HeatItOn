using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Frontend.Models;

namespace Frontend.Data;

public class OptimizerClient
{
    private readonly HttpClient _client;
    
    public OptimizerClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<HttpResponseMessage> Optimize(List<Asset> scenarioAssets)
    {
        var result = await _client.PostAsync("Optimize", JsonContent.Create(scenarioAssets));
        return result;
    }
    public async Task<HttpResponseMessage> Optimize()//remove before flight
    {
        var result = await _client.PostAsync("Optimize", null);
        return result;
    }
}