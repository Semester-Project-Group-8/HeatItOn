using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.Controllers
{
    [Route("Optimize")]
    [ApiController]
    public class OptimizerController : ControllerBase
    {
        private readonly OptimizerService _optimizerService;
        private readonly IHubContext<BackendHub> _hubContext;

        public OptimizerController(OptimizerService optimizerService, IHubContext<BackendHub> hubContext)
        {
            _optimizerService = optimizerService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<ActionResult<OptimizedResults>> Optimize([FromBody] List<Asset> scenarioAssets)
        {
            if(scenarioAssets.Count==0)
            {
                return BadRequest("BadRequest | Must have at least one Asset to Optimize.");
            }
            try
            {
                var result =  await _optimizerService.Optimize(scenarioAssets);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Optimized");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
    }
}
