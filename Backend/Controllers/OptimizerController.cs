using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.Controllers
{
    [Route("Optimize")]
    [ApiController]
    public class OptimizerController : ControllerBase
    {
        private readonly OptimizerService _optimizerService;

        public OptimizerController(OptimizerService optimizerService)
        {
            _optimizerService = optimizerService;
        }

        [HttpGet("{id:int}-{date:datetime}")]
        public async Task<IActionResult> NetProductionCost(int assetId, DateTime date)
        {
            try
            {
                float cost = await _optimizerService.CalculateNetProductionCost(assetId,date);
                return Ok(cost);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<ResultList>>> Optimize()
        {
            try
            {
                return await _optimizerService.Optimize();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
    }
}
