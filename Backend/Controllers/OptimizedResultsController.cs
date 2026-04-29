using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("OptimizedResults")]
[ApiController]
public class OptimizedResultsController : ControllerBase
{
    private readonly OptimizedResultsService _optimizedResultsService;
    
    public OptimizedResultsController(OptimizedResultsService optimizedResultsService)
    {
        _optimizedResultsService = optimizedResultsService;
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
        return Ok(result);
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _optimizedResultsService.Delete(id);
        return Ok("deleted");
    }
}