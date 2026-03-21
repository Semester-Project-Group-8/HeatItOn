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

        public async Task<IActionResult> Optimize(List<Source> AllSources, List<Asset> ScenarioAssets)//Task<List<ResultList>>
        {
            List<(Asset asset, float cost)> prices = new();
            int maintencance = 45;
            foreach (var source in AllSources)
            {
                prices.Clear();

                foreach (var asset in ScenarioAssets)
                {
                    //if (asset.Id == 2 && maintencance != 0)//if generator no.2 under maintenance period skip
                    {
                        //continue;
                    }
                    float cost = await CalculateNetProductionCost(asset.Id, source.TimeFrom);
                    prices.Add((asset, cost));
                }
                prices.Sort((a, b) => a.cost.CompareTo(b.cost));
                float metEnergy = 0;
                int usedGenerators = 0;
                while(metEnergy < source.HeatDemand)
                {
                    metEnergy += prices[usedGenerators].asset.MaxHeat;
                    usedGenerators++;
                    //create result
                }

                //create result list
                //add resultlist to list of result list
                maintencance--;
            }

            return null;
        }
    }
}
