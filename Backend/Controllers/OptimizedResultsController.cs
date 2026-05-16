using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[Route("OptimizedResults")]
[ApiController]
public class OptimizedResultsController : ControllerBase, IController<OptimizedResults, OptimizedResults>
{
    private readonly OptimizedResultsService _optimizedResultsService;
    private readonly IHubContext<BackendHub> _hubContext;

    public OptimizedResultsController(OptimizedResultsService optimizedResultsService,
        IHubContext<BackendHub> hubContext)
    {
        _optimizedResultsService = optimizedResultsService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _optimizedResultsService.List();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var results = await _optimizedResultsService.Get(id);
        return Ok(results);
    }

    [HttpPost("Add")]
    public Task<IActionResult> Post([FromBody] OptimizedResults optimizedResults)
    {
        throw new UnauthorizedAccessException("You are not authorized to create it.");
    }

    [HttpPut("Update/{id:int}")]
    public Task<IActionResult> Put(int id, [FromBody] OptimizedResults optimizedResults)
    {
        throw new  UnauthorizedAccessException("You are not authorized to modify it.");
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _optimizedResultsService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "OptimizedResults");
        return Ok("deleted");
    }
}