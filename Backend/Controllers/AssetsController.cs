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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAsset(int id)
        {
            try
            {
                var asset = await _assetsService.GetAsset(id);
                return Ok(asset);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsset([FromBody] Asset asset)
        {
            try
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
                    MaxElectricity = asset.MaxElectricity,
                    ImageId = asset.ImageId,
                    Image = asset.Image
                };
                await _assetsService.AddAsset(a.Id, a.Name, a.MaxHeat, a.ProductionCost, a.CO2Emission, a.GasConsumption, a.OilConsumption, a.MaxElectricity, a.ImageId, a.Image);
                return Created($"/Asset/{a.Id}", new { Id = a.Id, Name = a.Name, MaxHeat = a.MaxHeat, ProductionCost = a.ProductionCost, CO2Emission = a.CO2Emission, GasConsumption = a.GasConsumption, OilConsumption = a.OilConsumption, MaxElectricity = a.MaxElectricity, ImageId = a.ImageId });
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                await _assetsService.DeleteAsset(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset asset)
        {
            try
            {
                await _assetsService.UpdateAsset(id, asset.Name, asset.MaxHeat, asset.ProductionCost, asset.CO2Emission, asset.GasConsumption, asset.OilConsumption, asset.MaxElectricity, asset.ImageId, asset.Image);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }



}