using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
namespace Backend.Controllers
{
    [Route("Source")]
    [ApiController]
    public class SourceController : ControllerBase, IController<Source, Source>
    {
        private readonly SourceService _sourceService;
        private readonly IHubContext<BackendHub> _hubContext;
        public SourceController(SourceService sourceService, IHubContext<BackendHub> hubContext)
        {
            _sourceService = sourceService;
            _hubContext = hubContext;
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
                await _sourceService.Post(source);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
                return Created();
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
        [HttpGet("Month/{month:int}")]
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
        public async Task<IActionResult> Put(int id, [FromBody] Source source)
        {
            try
            {
                await _sourceService.Put(id, source);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _sourceService.Delete(id);
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

