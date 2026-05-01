using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
namespace Backend.Controllers
{
    [Route("ResultList")]
    [ApiController]

    public class ResultListController : ControllerBase, IController<ResultList, ResultList>
    {
        private readonly ResultListService _resultListService;
        private readonly IHubContext<BackendHub> _hubContext;
        public ResultListController(ResultListService resultListService, IHubContext<BackendHub> hubContext)
        {
            _resultListService = resultListService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var resultLists = await _resultListService.ListResultLists();
                return Ok(resultLists);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var resultList = await _resultListService.GetResultList(id);
                return Ok(resultList);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        [HttpPost("Add")]
        public async Task<IActionResult> Post([FromBody] ResultList resultList)
        {
            List<ResultList> list = new List<ResultList> { resultList };
            var result = await _resultListService.AddResultList(list);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "ResultList");
            return Ok(result);
        }

        [HttpPost("Adds")]
        public async Task<IActionResult> AddResultLists([FromBody] List<ResultList> resultLists)
        {
            var result = await _resultListService.AddResultList(resultLists);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "ResultList");
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public Task<IActionResult> Put(int id, [FromBody] ResultList resultList)
        {
            return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status501NotImplemented, new { message = "Update is not supported for ResultList." }));
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateResultList([FromQuery] DateTime timeFrom, [FromQuery] DateTime timeTo, [FromBody] List<Result> resultList)
        {
            try
            {
                var createdId = await _resultListService.CreateResultList(timeFrom, timeTo, resultList);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "ResultList");
                return Created($"/ResultList/{createdId}", new { id = createdId });
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _resultListService.DeleteResultList(id);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "ResultList");
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}