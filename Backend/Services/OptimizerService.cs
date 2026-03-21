using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Services
{
    public class OptimizerService
    {
        AssetsService _assetService;
        SourceService _sourceService;
        //ResultService _resultService;
        //ResultListService _resultListService

        public OptimizerService(AssetsService assetService, SourceService sourceService)
        {
            _assetService = assetService;
            _sourceService = sourceService;
            //_resultService = resultService;
            //_resultListService = resultListService;
        }

        public async Task<float> CalculateNetProductionCost(int assetId, DateTime date)
        {
            Asset asset = await _assetService.GetAsset(assetId);
            float netProductionCost = asset.ProductionCost * asset.MaxHeat;
            if (asset.MaxElectricity < 0)
            {
                Source source = await _sourceService.ListByHour(date);
                netProductionCost += source.ElectricityPrice * asset.MaxElectricity * -1;
            }
            if (asset.MaxElectricity > 0)
            {
                Source source = await _sourceService.ListByHour(date);
                netProductionCost -= source.ElectricityPrice * asset.MaxElectricity;
            }
            return netProductionCost;
        }

        public async Task<IActionResult> Optimize(List<Source> AllSources, List<Asset> ScenarioAssets)
        {
            Dictionary<Asset, Task<float>> price = new();
            foreach (var asset in ScenarioAssets)
            {
                price.Add(asset, Task.FromResult(0f));
            }
            foreach (var source in AllSources)
            {
                foreach (var asset in ScenarioAssets)
                {
                    price[asset] = CalculateNetProductionCost(asset.Id, source.TimeFrom);
                }
            }
            var sorted = (await Task.WhenAll(
                price.Select(async kvp => new KeyValuePair<Asset, float>(kvp.Key, await kvp.Value))
            ))
            .OrderBy(kvp => kvp.Value)
            .ToList();
            return null;
        }
    }
}
