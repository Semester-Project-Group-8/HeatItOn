using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Result")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly ResultService _resultService;
        public ResultController(ResultService ResultService)
        {
            _resultService = ResultService;
        }

        // GetAllResults
        [HttpGet]
        public async Task<IActionResult> GetAllResults()
        {
            var Results = await _resultService.ListResult();
            if (Results == null)
            {
                return NotFound("No results found.");
            }
            return Ok(Results);
        }

        // GetResultById
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResult(int id)
        {
            try
            {
                var result = await _resultService.GetResultById(id);
                return Ok(result);
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

        // AddResult ( for a single result )
        [HttpPost("Add")]
        public async Task<IActionResult> AddResult([FromBody] Result incomingResult) // Renamed to avoid collision
        {
            var rowsAffected = await _resultService.AddResult(
                incomingResult.Id,
                incomingResult.HeatProduction,
                incomingResult.Electricity,
                incomingResult.ProductionCost,
                incomingResult.PrimaryEnergyConsumed,
                incomingResult.CO2Produced,
                incomingResult.AssetId,
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
            var rowsAffected = await _resultService.AddResult(results);
            if (rowsAffected > 0)
            {
                return Ok(new { Message = $"{results.Count} results added successfully." });
            }

            return BadRequest("Failed to add the list of results.");
        }

        // UpdateResult
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] Result incomingResult) // Renamed
        {
            try
            {
                var rowsAffected = await _resultService.UpdateResult(
                    incomingResult.Id,
                    incomingResult.HeatProduction,
                    incomingResult.Electricity,
                    incomingResult.ProductionCost,
                    incomingResult.PrimaryEnergyConsumed,
                    incomingResult.CO2Produced,
                    incomingResult.AssetId,
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

        // DeleteResult
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            try
            {
                var rowsAffected = await _resultService.DeleteResult(id);
                if (rowsAffected > 0)
                {
                    return Ok(new { Message = "Result deleted successfully." });
                }
                return BadRequest("Failed to delete Result.");
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
