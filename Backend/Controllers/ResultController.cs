using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
namespace Backend.Controllers
{
    [Route("Result")]
    [ApiController]
    public class ResultController : ControllerBase, IController<Result, Result>
    {
        private readonly ResultService _resultService;
        private readonly IHubContext<BackendHub> _hubContext;
        public ResultController(ResultService ResultService, IHubContext<BackendHub> hubContext)
        {
            _resultService = ResultService;
            _hubContext = hubContext;
        }

        // List
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var Results = await _resultService.List();
            if (Results == null)
            {
                return NotFound("No results found.");
            }
            return Ok(Results);
        }

        // Get
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var results = await _resultService.Get(id);
                return Ok(results.First());
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GetResultByAssetId
        [HttpGet("Asset/{assetId:int}")]
        public async Task<IActionResult> GetResultByAsset(int assetId)
        {
            try
            {
                var result = await _resultService.GetResultByAssetId(assetId);
                return Ok(result);
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Post (for a single result)
        [HttpPost("Add")]
        public async Task<IActionResult> Post([FromBody] Result incomingResult)
        {
            var rowsAffected = await _resultService.AddResult(
                incomingResult.Id,
                incomingResult.HeatProduction,
                incomingResult.Electricity,
                incomingResult.ProductionCost,
                incomingResult.PrimaryEnergyConsumed,
                incomingResult.CO2Produced,
                incomingResult.AssetId
            );

            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
                return Created($"/Result/{incomingResult.Id}", incomingResult);
            }

            return BadRequest("Failed to add Result.");
        }

        // AddResultList
        [HttpPost("AddList")]
        public async Task<IActionResult> AddResultList([FromBody] List<Result> results)
        {
            var rowsAffected = await _resultService.AddResult(results);
            if (rowsAffected > 0)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
                return Ok(new { Message = $"{results.Count} results added successfully." });
            }

            return BadRequest("Failed to add the list of results.");
        }

        // Put
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Result incomingResult)
        {
            try
            {
                var rowsAffected = await _resultService.UpdateResult(
                    id,
                    incomingResult.HeatProduction,
                    incomingResult.Electricity,
                    incomingResult.ProductionCost,
                    incomingResult.PrimaryEnergyConsumed,
                    incomingResult.CO2Produced,
                    incomingResult.AssetId
                );

                if (rowsAffected > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
                    return Ok(new { Message = "Result updated successfully." });
                }
                return BadRequest("Failed to update Result.");
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Delete
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
         
                await _resultService.Delete(id);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
                return Ok(new { Message = "Result deleted successfully." });
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
