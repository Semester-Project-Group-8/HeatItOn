using Backend.Models;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<float> CalculateNetProductionCost(int assetId,DateTime date)
        {
            Asset asset = await _assetService.GetAsset(assetId);
            float netProductionCost = asset.ProductionCost;
            if(asset.MaxElectricity<0)
            {
                Source source = await _sourceService.ListByHour(date);
                netProductionCost += source.ElectricityPrice*asset.MaxElectricity*1;
            }
            return netProductionCost;
        }

        public Task<IActionResult> Optimize()
        {
            return null;
        }
    }
}
