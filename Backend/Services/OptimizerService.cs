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
        readonly ResultByHourService _resultListService;
        readonly OptimizedResultsService _optimizedResultsService;

        public OptimizerService(AssetsService assetService, SourceService sourceService,ResultService resultService,ResultByHourService resultListService, OptimizedResultsService optimizedResultsService)
        {
            _assetService = assetService;
            _sourceService = sourceService;
            _resultService = resultService;
            _resultListService = resultListService;
            _optimizedResultsService = optimizedResultsService;
        }
        private int DeterminePrimaryEnergyConsumed(Asset asset)
        {
            if (asset.ProductionCost == 0)
                return 3;//heat storage
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
            Asset? PTES;//Pit Thermal Energy Storage
            PTES = scenarioAssets.FirstOrDefault(asset => asset.ProductionCost == 0);
            if (PTES != null)
            {
                //scenarioAssets.Remove(PTES);
            }
            var allSources = (await _sourceService.List())
                .OrderBy(s => s.TimeFrom)
                .ToList();
            OptimizedResults finalResults = new OptimizedResults
            {
                Name = $"results_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}",
                ResultsForHours = new List<ResultByHour>()
            };
            foreach (Source source in allSources)
            {
                scenarioAssets=SortAssets(scenarioAssets,source.ElectricityPrice);
                float heatOfTheHour = 0;
                int usedGenerators = 0;
                bool allowOverproduction = true;
                DateTime maintenanceFrom = DateTime.Parse("2025-09-09T20:00:00");
                DateTime maintenanceTil = maintenanceFrom.AddHours(45);
                ResultByHour resultOfHour = new ResultByHour
                {
                    TimeFrom = source.TimeFrom,
                    TimeTo = source.TimeTo,
                    Results = new List<Result>()
                };
                while(usedGenerators<scenarioAssets.Count() && (heatOfTheHour < source.HeatDemand || allowOverproduction))
                {
                    if(scenarioAssets[usedGenerators].Name=="Gas boiler 1" && maintenanceFrom <= source.TimeFrom && source.TimeFrom < maintenanceTil)//maintenance
                    {
                        usedGenerators++;
                        continue;
                    }
                    float targetHeatProduction = 0;
                    if (source.HeatDemand - heatOfTheHour > scenarioAssets[usedGenerators].MaxHeat || heatOfTheHour >= source.HeatDemand)//demand higher than asset can generate
                    {
                        targetHeatProduction = scenarioAssets[usedGenerators].MaxHeat;
                    }
                    else//check if we profit by overproduction
                    {
                        targetHeatProduction = source.HeatDemand - heatOfTheHour;
                        Result partialProductionResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, targetHeatProduction);
                        Result fullProductionResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, scenarioAssets[usedGenerators].MaxHeat);
                        if(fullProductionResult.ProductionCost<partialProductionResult.ProductionCost)
                        {
                            targetHeatProduction = scenarioAssets[usedGenerators].MaxHeat;
                        }
                    }
                    if(PTES != null && scenarioAssets[usedGenerators] == PTES && PTES.MaxHeat==0)//if we dont store any heat skipp
                    {
                        usedGenerators++;
                        continue;
                    }
                    Result newResult = CalculateAssetResult(scenarioAssets[usedGenerators], source, targetHeatProduction);
                    if (heatOfTheHour >= source.HeatDemand && newResult.ProductionCost >= 0)//if we met demand or if we dont make money by overproducing stop
                    {
                        allowOverproduction=false;
                    }
                    if(allowOverproduction || (PTES != null && scenarioAssets[usedGenerators] == PTES && heatOfTheHour<source.HeatDemand))
                    {
                        if(PTES != null && scenarioAssets[usedGenerators] == PTES)//if we used heat from storage substract
                        {
                            PTES.MaxHeat -= targetHeatProduction;
                        }
                        resultOfHour.Results.Add(newResult);
                        heatOfTheHour += targetHeatProduction;
                        usedGenerators++;
                    }
                }
                if(PTES!=null && heatOfTheHour>source.HeatDemand)//save extra heat
                {
                    float overProduction = heatOfTheHour - source.HeatDemand;
                    if(overProduction>30)
                    {
                        overProduction = 30;//Max transfer is 30 MWh
                    }
                    if(PTES.MaxHeat+overProduction<=30000)//Max capacity is 30000MW(th)
                    {
                        PTES.MaxHeat += overProduction;
                    }
                }
                finalResults.ResultsForHours.Add(resultOfHour);
            }
            await _resultListService.PostList(finalResults.ResultsForHours);
            await _optimizedResultsService.Post(finalResults);
            return finalResults;
        }
    }
}
