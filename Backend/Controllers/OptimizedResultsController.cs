using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[Route("OptimizedResults")]
[ApiController]
public class OptimizedResultsController : ControllerBase
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
        var result = await _optimizedResultsService.ListOptimizedResults();
        return Ok(result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _optimizedResultsService.GetOptimizedResults(id);
        return Ok(result);
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Post([FromBody] OptimizedResults optimizedResults)
    { 
        var result = await _optimizedResultsService.AddOptimizedResults(optimizedResults);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "OptimizedResults");
        return Ok(result);
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _optimizedResultsService.DeleteOptimizedResult(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "OptimizedResults");
        return Ok("deleted");
    }
}