using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[Route("Asset")]
[ApiController]
public class AssetsController : ControllerBase, IController<Asset, Asset>
{
    private readonly AssetsService _assetsService;
    private readonly IHubContext<BackendHub> _hubContext;

    public AssetsController(AssetsService assetsService, IHubContext<BackendHub> hubContext)
    {
        _assetsService = assetsService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var assets = await _assetsService.List();
        return Ok(assets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var assets = await _assetsService.Get(id);
        return Ok(assets);
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Post([FromBody] Asset asset)
    {
        // Checking received data validity
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _assetsService.Post(asset);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
        return Created();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _assetsService.Delete(id);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Asset asset)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        await _assetsService.Put(id, asset);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Asset");
        return Ok();
    }
}