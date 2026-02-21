using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Demand")]
    [ApiController]
    public class DemandController : ControllerBase
    {
        private readonly DemandService _demandService;
        public DemandController(DemandService DemandService)
        {
            _demandService = DemandService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDemands()
        {
            var Demands = await _demandService.ListDemands();
            return Ok(Demands);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddDemand([FromBody] Demand Demand)
        {
            Demand d = new Demand
            {
                ID = Demand.ID,
                StartTime = Demand.StartTime,
                EndTime = Demand.EndTime,
                HeatDemand = Demand.HeatDemand,
                ElectricityPrice = Demand.ElectricityPrice
            };
            var result = await _demandService.AddDemand(d.ID, d.StartTime, d.EndTime, d.HeatDemand, d.ElectricityPrice);
            if (result > 0)
            {
                return Created($"/Demand/{d.ID}", new { ID = d.ID, StartTime = d.StartTime, EndTime = d.EndTime, HeatDemand = d.HeatDemand, ElectricityPrice = d.ElectricityPrice });
            }
            else
            {
                return BadRequest("Failed to add Demand.");
            }
        }
    }
}
