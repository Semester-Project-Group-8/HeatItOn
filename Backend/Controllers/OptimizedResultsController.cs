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
        var result = await _optimizedResultsService.ListOptimizedResults();
        return Ok(result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _optimizedResultsService.GetOptimizedResults(id);
        return Ok(result);
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _optimizedResultsService.DeleteOptimizedResult(id);
        return Ok("deleted");
    }
}