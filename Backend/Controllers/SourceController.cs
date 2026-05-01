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
                var Sources = await _sourceService.List();
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
                var source = await _sourceService.Get(id);
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
                await _sourceService.Post([s]);
                return Created($"/Source/{s.Id}", new { Id = s.Id, TimeFrom = s.TimeFrom, TimeTo = s.TimeTo, HeatDemand = s.HeatDemand, ElectricityPrice = s.ElectricityPrice });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        
        
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Source source)
        {
            try
            {
                await _sourceService.Put(id, source);
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
                await _sourceService.Delete(id);
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

