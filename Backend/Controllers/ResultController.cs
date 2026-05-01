using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Result")]
    [ApiController]
    public class ResultController : ControllerBase, IController<Result, Result>
    {
        private readonly ResultService _resultService;
        public ResultController(ResultService ResultService)
        {
            _resultService = ResultService;
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
                return Ok(results);
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GetResultByAssetId
        [HttpGet("Asset/{assetId:int}")]
        public async Task<IActionResult> GetByAssetId(int assetId)
        {
            try
            {
                var result = await _resultService.Get(assetId);
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
            var rowsAffected = await _resultService.Post(
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
                return Created($"/Result/{incomingResult.Id}", incomingResult);
            }

            return BadRequest("Failed to add Result.");
        }

        // AddResultList
        [HttpPost("AddList")]
        public async Task<IActionResult> AddResultList([FromBody] List<Result> results)
        {
            var rowsAffected = await _resultService.Post(results);
            if (rowsAffected > 0)
            {
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
                var rowsAffected = await _resultService.Post(
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
                return Ok(new { Message = "Result deleted successfully." });
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
