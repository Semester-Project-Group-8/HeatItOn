using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("Source")]
    [ApiController]
    public class SourceController : ControllerBase, IController<Source, Source>
    {
        private readonly SourceService _sourceService;
        public SourceController(SourceService SourceService)
        {
            _sourceService = SourceService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var Sources = await _sourceService.ListSources();
                return Ok(Sources);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var source = await _sourceService.GetSource(id);
                return Ok(source);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Post([FromBody] Source source)
        {
            try
            {
                Source s = new Source
                {
                    Id = source.Id,
                    TimeFrom = source.TimeFrom,
                    TimeTo = source.TimeTo,
                    HeatDemand = source.HeatDemand,
                    ElectricityPrice = source.ElectricityPrice
                };
                await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
                return Created($"/Source/{s.Id}", new { Id = s.Id, TimeFrom = s.TimeFrom, TimeTo = s.TimeTo, HeatDemand = s.HeatDemand, ElectricityPrice = s.ElectricityPrice });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpGet("Month/{month:int}")]
        public async Task<IActionResult> GetByMonth(int month)
        {
            try
            {
                var Sources = await _sourceService.ListByMonth(month);
                return Ok(Sources);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpGet("{date:DateTime}")]
        public async Task<IActionResult> GetByHour(DateTime date)
        {
            var Sources = await _sourceService.ListByHour(date);
            return Ok(Sources);
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Source source)
        {
            try
            {
                await _sourceService.UpdateSource(id, source.TimeFrom, source.TimeTo, source.HeatDemand, source.ElectricityPrice);
                return Ok(new { Message = "Source updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _sourceService.DeleteSource(id);
                return Ok(new { Message = "Source deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}

