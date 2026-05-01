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
        readonly ResultService _resultService;
        readonly ResultListService _resultListService;
        readonly OptimizedResultsService _optimizedResultsService;

        public OptimizerService(AssetsService assetService, SourceService sourceService,ResultService resultService,ResultListService resultListService, OptimizedResultsService optimizedResultsService)
        {
            _assetService = assetService;
            _sourceService = sourceService;
            _resultService = resultService;
            _resultListService = resultListService;
            _optimizedResultsService = optimizedResultsService;
        }
        private int DeterminePrimaryEnergyConsumed(Asset asset)
        {
            if (asset.GasConsumption > asset.OilConsumption && -asset.GasConsumption < asset.MaxElectricity)
                return 0;//gas
            if (asset.OilConsumption > asset.GasConsumption && -asset.OilConsumption < asset.MaxElectricity)
                return 1;//oil
            return 2;//electricity
        }
        public Result CalculateAssetResult(Asset asset, Source source,float heatTarget)
        {
            float assetProductionCapacity = heatTarget / asset.MaxHeat;//determiunes the capacity the asset is running at
            Result result = new Result()
            {
                HeatProduction = heatTarget,
                PrimaryEnergyConsumed = DeterminePrimaryEnergyConsumed(asset),
                CO2Produced=Convert.ToInt32(asset.CO2Emission*assetProductionCapacity),
                AssetId=asset.Id
            };
            float netProductionCost = asset.ProductionCost * assetProductionCapacity* asset.MaxHeat;
            netProductionCost -= asset.MaxElectricity * assetProductionCapacity * source.ElectricityPrice;
            result.ProductionCost = netProductionCost;
            result.Electricity =asset.MaxElectricity * assetProductionCapacity;
            return result;
        }
        private List<Asset> SortAssets(List<Asset> assets,float electricityPrice)
        {
            return assets.OrderBy(asset => asset.ProductionCost - asset.MaxElectricity/asset.MaxHeat * electricityPrice).ToList();
        }
        public async Task<ActionResult<OptimizedResults>> Optimize(List<Asset> scenarioAssets)
        {
            var allSources = (await _sourceService.List())
                .OrderBy(s => s.TimeFrom)
                .ToList();
            OptimizedResults finalResults = new OptimizedResults
            {
                Name = $"results_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}",
                ResultsForHours = new List<ResultList>()
            };
            foreach (Source source in allSources)
            {
                scenarioAssets=SortAssets(scenarioAssets,source.ElectricityPrice);
                float heatOfTheHour = 0;
                int usedGenerators = 0;
                bool allowOverproduction = true;
                DateTime maintenanceFrom = DateTime.Parse("2025-09-09T20:00:00");
                DateTime maintenanceTil = maintenanceFrom.AddHours(45);
                ResultList resultOfHour = new ResultList
                {
                    TimeFrom = source.TimeFrom,
                    TimeTo = source.TimeTo,
                    Results = new List<Result>()
                };
                while(usedGenerators<scenarioAssets.Count() && (heatOfTheHour < source.HeatDemand || allowOverproduction))
                {
                    if(scenarioAssets[usedGenerators].Name=="Gas boiler 1" && maintenanceFrom <= source.TimeFrom && source.TimeFrom < maintenanceTil)
                    {
                        usedGenerators++;
                        continue;
                    }
                    float targetHeatProduction = 0;
                    if (source.HeatDemand - heatOfTheHour > scenarioAssets[usedGenerators].MaxHeat || heatOfTheHour >= source.HeatDemand)
                    {
                        targetHeatProduction = scenarioAssets[usedGenerators].MaxHeat;
                    }
                    else
                    {
                        targetHeatProduction = source.HeatDemand - heatOfTheHour;
                        Result partialProductionResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, targetHeatProduction);
                        Result fullProductionResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, scenarioAssets[usedGenerators].MaxHeat);
                        if(fullProductionResult.ProductionCost<partialProductionResult.ProductionCost)
                        {
                            targetHeatProduction = scenarioAssets[usedGenerators].MaxHeat;
                        }
                    }
                    Result newResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, targetHeatProduction);
                    if (heatOfTheHour >= source.HeatDemand && newResult.ProductionCost >= 0)
                    {
                        allowOverproduction=false;
                    }
                    if(allowOverproduction)
                    {
                        resultOfHour.Results.Add(newResult);
                        heatOfTheHour += targetHeatProduction;
                        usedGenerators++;
                    }
                }
                finalResults.ResultsForHours.Add(resultOfHour);
            }
            await _resultListService.AddResultList(finalResults.ResultsForHours);
            await _optimizedResultsService.AddOptimizedResults(finalResults);
            return finalResults;
        }

        /*
        public async Task<float> CalculateNetProductionCost(int assetId, DateTime date, float heatTarget)
        {
            Asset asset = await _assetService.GetAsset(assetId);
            float generatedHeat = heatTarget;
            if (generatedHeat > asset.MaxHeat)
            {
                generatedHeat = asset.MaxHeat;
            }
            float netProductionCost = asset.ProductionCost * generatedHeat;
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

        public async Task<ActionResult<OptimizedResults>> Optimize(List<Asset> ScenarioAssets)//Task<List<ResultList>>
        {
            var AllSources = await _sourceService.ListSources();
            //var ScenarioAssets = await _assetService.ListAssets();
            List<(Asset asset, float cost)> prices = new();
            List<ResultList> optimizedData = new List<ResultList>();
            int maintencance = 0;
            foreach (var source in AllSources)
            {
                ResultList resultOfHour=new ResultList();
                resultOfHour.TimeFrom = source.TimeFrom;
                resultOfHour.TimeTo = source.TimeTo;
                prices.Clear();

                foreach (var asset in ScenarioAssets)
                {
                    if (asset.Id == 2 && maintencance > 0)//if generator no.2 under maintenance period skip
                    {
                        continue;
                    }
                    float cost = await CalculateNetProductionCost(asset.Id, source.TimeFrom);
                    prices.Add((asset, cost));
                }
                prices.Sort((a, b) => a.cost.CompareTo(b.cost));
                float metEnergy = 0;
                int usedGenerators = 0;
                while (metEnergy < source.HeatDemand && usedGenerators<=prices.Count())
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
                await _resultService.AddResult(resultOfHour.Results);//change when AddResult exists
                optimizedData.Add(resultOfHour);
                maintencance--;
            }
            await _resultListService.AddResultList(optimizedData);
            OptimizedResults optimizedResult = new OptimizedResults
            {
                //Id= 1,
                Name = $"result_{DateTime.Now:yyyy_MM_dd_HH_mm}",
                ResultsForHours= optimizedData
            };
            await _optimizedResultsService.AddOptimizedResults(optimizedResult);
            return optimizedResult;
        }*/
    }
}
