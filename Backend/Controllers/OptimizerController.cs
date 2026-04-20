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

        [HttpPost]
        public async Task<ActionResult<OptimizedResults>> Optimize([FromBody] List<Asset> scenarioAssets)
        {
            if(scenarioAssets == null || scenarioAssets.Count==0)
            {
                return BadRequest("BadRequest | Must have at least one Asset to Optimize.");
            }
            try
            {
                return await _optimizerService.Optimize(scenarioAssets);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
    }
}
