using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("OptimizedResults")]
[ApiController]
public class OptimizedResultsController : ControllerBase, IController<OptimizedResults, OptimizedResults>
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
        return Ok("deleted");
    }
}