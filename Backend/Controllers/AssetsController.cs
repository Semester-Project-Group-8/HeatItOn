using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers
{
    [Route("Asset")]
    [ApiController]

    public class AssetsController : ControllerBase, IController<Asset, Asset>
    {
        private readonly AssetsService _assetsService;
        private readonly IHubContext<BackendHub> _hubContext;
        public AssetsController(AssetsService assetsService, IHubContext<BackendHub> hubContext)
        {
            _assetsService = assetsService;
            _hubContext = hubContext;
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
                await _assetsService.Post(asset.Id, asset.Name, asset.MaxHeat, asset.ProductionCost, asset.CO2Emission, asset.GasConsumption, asset.OilConsumption, asset.MaxElectricity, asset.ImageName);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
                return Created();
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
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Asset asset)
        {
            try
            {
                await _assetsService.Put(id, asset);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
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
