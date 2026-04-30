using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
namespace Backend.Controllers
{
    [Route("Source")]
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly SourceService _sourceService;
        private readonly IHubContext<BackendHub> _hubContext;
        public SourceController(SourceService sourceService, IHubContext<BackendHub> hubContext)
        {
            _sourceService = sourceService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSources()
        {
            try
            {
                var sources = await _sourceService.ListSources();
                return Ok(sources);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddSource([FromBody] Source source)
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
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
                return Created($"/Source/{s.Id}", new { s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand,
                    s.ElectricityPrice });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("AddList")]
        public async Task<IActionResult> AddSources([FromBody] List<Source> sources)
        {
            try
            {
                await _sourceService.AddSources(sources);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
                return Created();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [HttpGet("{month:int}")]
        public async Task<IActionResult> GetByMonth(int month)
        {
            try
            {
                var sources = await _sourceService.ListByMonth(month);
                return Ok(sources);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        [HttpGet("{date:DateTime}")]
        public async Task<IActionResult> GetByHour(DateTime date)
        {
            var sources = await _sourceService.ListByHour(date);
            return Ok(sources);
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> UpdateSource(int id, [FromBody] Source source)
        {
            try
            {
                await _sourceService.UpdateSource(id, source.TimeFrom, source.TimeTo, source.HeatDemand, source.ElectricityPrice);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
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
        public async Task<IActionResult> DeleteSource(int id)
        {
            try
            {
                await _sourceService.DeleteSource(id);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
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

