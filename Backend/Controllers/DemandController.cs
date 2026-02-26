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
        public async Task<IActionResult> AddDemand([FromBody] Demand demand)
        {
            Demand d = new Demand
            {
                ID = demand.ID,
                StartTime = demand.StartTime,
                EndTime = demand.EndTime,
                HeatDemand = demand.HeatDemand,
                ElectricityPrice = demand.ElectricityPrice
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
        [HttpGet("{month:int}")]
        public async Task<IActionResult> GetByMonth(int month)
        {
            var Demands = await _demandService.ListByMonth(month);
            return Ok(Demands);
        }
    }
}
