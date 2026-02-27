using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Source")]
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly SourceService _sourceService;
        public SourceController(SourceService SourceService)
        {
            _sourceService = SourceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSources()
        {
            var Sources = await _sourceService.ListSources();
            return Ok(Sources);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddSource([FromBody] Source source)
        {
            Source s = new Source
            {
                ID = source.ID,
                StartTime = source.StartTime,
                EndTime = source.EndTime,
                HeatDemand = source.HeatDemand,
                ElectricityPrice = source.ElectricityPrice
            };
            var result = await _sourceService.AddSource(s.ID, s.StartTime, s.EndTime, s.HeatDemand, s.ElectricityPrice);
            if (result > 0)
            {
                return Created($"/Source/{s.ID}", new { ID = s.ID, StartTime = s.StartTime, EndTime = s.EndTime, HeatDemand = s.HeatDemand, ElectricityPrice = s.ElectricityPrice });
            }
            else
            {
                return BadRequest("Failed to add Source.");

            }
        }
        [HttpGet("{month:int}")]
        public async Task<IActionResult> GetByMonth(int month)
        {
            var Sources = await _sourceService.ListByMonth(month);
            return Ok(Sources);
        }
    }
}
