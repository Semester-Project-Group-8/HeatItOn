using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

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
        var Sources = await _sourceService.List();
        return Ok(Sources);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var source = await _sourceService.Get(id);
        return Ok(source);
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Post([FromBody] Source source)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _sourceService.Post(source);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
        return Created();
    }

    [HttpPost("AddList")]
    public async Task<IActionResult> AddSources([FromBody] List<Source> sources)
    {
        if (sources.Any(source => !ModelState.IsValid)) return BadRequest(ModelState);
        
        await _sourceService.PostList(sources);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
        return Created();
    }

    [HttpPut("Update/{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Source source)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _sourceService.Put(id, source);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
        return Ok(new { Message = "Source updated successfully." });
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sourceService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Source");
        return Ok(new { Message = "Source deleted successfully." });
    }
}