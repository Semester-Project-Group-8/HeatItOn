using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[Route("ResultList")]
[ApiController]
public class ResultByHourController : ControllerBase, IController<ResultByHour, ResultByHour>
{
    private readonly ResultByHourService _resultListService;
    private readonly IHubContext<BackendHub> _hubContext;

    public ResultByHourController(ResultByHourService resultListService, IHubContext<BackendHub> hubContext)
    {
        _resultListService = resultListService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var resultLists = await _resultListService.List();
        return Ok(resultLists);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var resultList = await _resultListService.Get(id);
        return Ok(resultList);
    }

    [HttpPost("Add")]
    public Task<IActionResult> Post([FromBody] ResultByHour result)
    {
        throw new UnauthorizedAccessException("You are not authorized to create it.");
    }

    [HttpPost("AddList")]
    public Task<IActionResult> AddResultLists([FromBody] List<Models.ResultByHour> results)
    {
        throw new UnauthorizedAccessException("You are not authorized to create it.");
    }

    [HttpPut("{id:int}")]
    public Task<IActionResult> Put(int id, [FromBody] Models.ResultByHour resultList)
    {
        throw new UnauthorizedAccessException("You are not authorized to modify it.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _resultListService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "ResultList");
        return NoContent();
    }
}