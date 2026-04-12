using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Services
{
    public class OptimizerService
    {
        readonly AssetsService _assetService;
        readonly SourceService _sourceService;
        readonly ResultListService _resultListService;

        public OptimizerService(AssetsService assetService, SourceService sourceService,ResultListService resultListService)
        {
            _assetService = assetService;
            _sourceService = sourceService;
            _resultListService = resultListService;
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

        public async Task<ActionResult<List<ResultList>>> Optimize()//Task<List<ResultList>>
        {
            var AllSources = await _sourceService.ListSources();
            var ScenarioAssets = await _assetService.ListAssets();
            List<(Asset asset, float cost)> prices = new();
            List<ResultList> optimizedData = new List<ResultList>();
            int maintencance = 45;
            foreach (var source in AllSources)
            {
                ResultList resultOfHour=new ResultList();
                resultOfHour.TimeFrom = source.TimeFrom;
                resultOfHour.TimeTo = source.TimeTo;
                prices.Clear();

                foreach (var asset in ScenarioAssets)
                {
                    if (asset.Id == 2 && maintencance != 0)//if generator no.2 under maintenance period skip
                    {
                        continue;
                    }
                    float cost = await CalculateNetProductionCost(asset.Id, source.TimeFrom);
                    prices.Add((asset, cost));
                }
                prices.Sort((a, b) => a.cost.CompareTo(b.cost));
                float metEnergy = 0;
                int usedGenerators = 0;
                while (metEnergy < source.HeatDemand)
                {
                    metEnergy += prices[usedGenerators].asset.MaxHeat;
                    Result result = new Result
                    {
                        //Id = prices[usedGenerators].asset.Id,
                        HeatProduction = prices[usedGenerators].asset.MaxHeat,
                        Electricity = prices[usedGenerators].asset.MaxElectricity,
                        ProductionCost = prices[usedGenerators].cost,
                        PrimaryEnergyConsumed = //0 gas 1 oil 2 electricity 3 other
                            prices[usedGenerators].asset.GasConsumption != 0 ? 0 :
                            prices[usedGenerators].asset.OilConsumption != 0 ? 1 :
                            prices[usedGenerators].asset.MaxElectricity > 0 ? 2 : 3,
                        CO2Produced = prices[usedGenerators].asset.CO2Emission,
                        AssetId = prices[usedGenerators].asset.Id
                    };
                    usedGenerators++;
                    resultOfHour.Results.Add(result);
                }
                optimizedData.Add(resultOfHour);
                maintencance--;
            }
            await _resultListService.AddResultList(optimizedData);
            return optimizedData;
        }
    }
}
