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
                Id = source.Id,
                TimeFrom = source.TimeFrom,
                TimeTo = source.TimeTo,
                HeatDemand = source.HeatDemand,
                ElectricityPrice = source.ElectricityPrice
            };
            var result = await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            if (result > 0)
            {
                return Created($"/Source/{s.Id}", new { Id = s.Id, TimeFrom = s.TimeFrom, TimeTo = s.TimeTo, HeatDemand = s.HeatDemand, ElectricityPrice = s.ElectricityPrice });
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
