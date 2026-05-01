using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Asset")]
    [ApiController]

    public class AssetsController : ControllerBase, IController<Asset, Asset>
    {
        private readonly AssetsService _assetsService;
        public AssetsController(AssetsService assetsService)
        {
            _assetsService = assetsService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var assets = await _assetsService.List();
                return Ok(assets);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var assets = await _assetsService.Get(id);
                return Ok(assets);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Post([FromBody] Asset asset)
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
                    ImageName = asset.ImageName
                };
                await _assetsService.AddAsset(a.Id, a.Name, a.MaxHeat, a.ProductionCost, a.CO2Emission, a.GasConsumption, a.OilConsumption, a.MaxElectricity, a.ImageName);
                return Created($"/Asset/{a.Id}", new { Id = a.Id, Name = a.Name, MaxHeat = a.MaxHeat, ProductionCost = a.ProductionCost, CO2Emission = a.CO2Emission, GasConsumption = a.GasConsumption, OilConsumption = a.OilConsumption});
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _assetsService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Asset asset)
        {
            try
            {
                await _assetsService.Put(id, asset);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
