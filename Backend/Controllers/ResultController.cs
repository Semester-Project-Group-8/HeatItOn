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
    public Task<IActionResult> Post([FromBody] Result incomingResult)
    {
        throw new UnauthorizedAccessException("You are not authorized to create it.");
    }

    // AddResultList
    [HttpPost("AddList")]
    public Task<IActionResult> AddResultList([FromBody] List<Result> results)
    {
        throw new UnauthorizedAccessException("You are not authorized to create it.");
    }

    // Put
    [HttpPut("Update/{id:int}")]
    public Task<IActionResult> Put(int id, [FromBody] Result result)
    {
        throw new UnauthorizedAccessException("You are not authorized to modify it.");
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