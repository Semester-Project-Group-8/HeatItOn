using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Asset")]
    [ApiController]

    public class AssetsController : ControllerBase
    {
        private readonly AssetsService _assetsService;
        public AssetsController(AssetsService assetsService)
        {
            _assetsService = assetsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssets()
        {
            var assets = await _assetsService.ListAssets();
            return Ok(assets);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsset([FromBody] Asset asset)
        {
            Asset a = new Asset
            {
                Id = asset.Id,
                Name = asset.Name,
                MaxHeat = asset.MaxHeat,
                ProductionCost = asset.ProductionCost,
                CO2Emission = asset.CO2Emission,
                GasConsumption = asset.GasConsumption,
                OilConsumption = asset.OilConsumption,
                MaxElectricicty = asset.MaxElectricicty,
                ImageId = asset.ImageId,
                Image = asset.Image
            };
            var result = await _assetsService.AddAsset(a.Id, a.Name, a.MaxHeat, a.ProductionCost, a.CO2Emission, a.GasConsumption, a.OilConsumption, a.MaxElectricicty, a.ImageId, a.Image);
            if (result > 0)
            {
                return Created($"/Asset/{a.Id}", new { Id = a.Id, Name = a.Name, MaxHeat = a.MaxHeat, ProductionCost = a.ProductionCost, CO2Emission = a.CO2Emission, GasConsumption = a.GasConsumption, OilConsumption = a.OilConsumption, MaxElectricity = a.MaxElectricicty, ImageId = a.ImageId });
            }
            else
            {
                return BadRequest("Failed to add Asset.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var result = await _assetsService.DeleteAsset(id);
            if (result > 0)
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Asset with ID {id} not found.");
            }
        }

        [HttpPut("{id:int}")]

        public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset asset)
        {
            var result = await _assetsService.UpdateAsset(id, asset.Name, asset.MaxHeat, asset.ProductionCost, asset.CO2Emission, asset.GasConsumption, asset.OilConsumption, asset.MaxElectricicty, asset.ImageId, asset.Image);
            if (result > 0)
            {
                return Ok($"Asset with ID {id} updated successfully.");
            }
            else
            {
                return NotFound($"Asset with ID {id} not found.");
            }
        }
    }



}