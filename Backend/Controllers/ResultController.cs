using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[Route("Result")]
[ApiController]
public class ResultController : ControllerBase, IController<Result, Result>
{
    private readonly ResultService _resultService;
    private readonly IHubContext<BackendHub> _hubContext;

    public ResultController(ResultService resultService, IHubContext<BackendHub> hubContext)
    {
        _resultService = resultService;
        _hubContext = hubContext;
    }

    // List
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var Results = await _resultService.List();
        return Ok(Results);
    }

    // Get
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var results = await _resultService.Get(id);
        return Ok(results);
    }

    // GetResultByAssetId
    [HttpGet("Asset/{assetId:int}")]
    public async Task<IActionResult> GetByAssetId(int assetId)
    {
        var result = await _resultService.Get(assetId);
        return Ok(result);
    }

    // Post (for a single result)
    [HttpPost("Add")]
    public async Task<IActionResult> Post([FromBody] Result incomingResult)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var created = _resultService.Post(incomingResult);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
        return Created($"/Result/{incomingResult.Id}", incomingResult);
    }

    // AddResultList
    [HttpPost("AddList")]
    public async Task<IActionResult> AddResultList([FromBody] List<Result> results)
    {
        if (results.Any(result => !ModelState.IsValid)) return BadRequest(ModelState);
        
        await _resultService.Post(results);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
        return Ok(new { Message = $"{results.Count} results added successfully." });
    }

    // Put
    [HttpPut("Update/{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Result result)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _resultService.Put(id, result);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
        return Ok(new { Message = "Result updated successfully." });
    }

    // Delete
    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _resultService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Result");
        return Ok(new { Message = "Result deleted successfully." });
    }
}