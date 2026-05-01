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
    
    public OptimizedResultsController(OptimizedResultsService optimizedResultsService, IHubContext<BackendHub> hubContext)
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
        return Ok(results.First());
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Post([FromBody] OptimizedResults optimizedResults)
    { 
        var result = await _optimizedResultsService.AddOptimizedResults(optimizedResults);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "OptimizedResults");
        return Ok(result);
    }

    [HttpPut("Update/{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] OptimizedResults optimizedResults)
    {
        optimizedResults.Id = id;
        var result = await _optimizedResultsService.UpdateOptimizedResults(optimizedResults);
        return Ok(result);
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _optimizedResultsService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "OptimizedResults");
        return Ok("deleted");
    }
}