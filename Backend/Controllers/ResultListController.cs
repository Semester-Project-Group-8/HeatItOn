using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [Route("ResultList")]
    [ApiController]

    public class ResultListController : ControllerBase
    {
        private readonly ResultListService _resultListService;
        public ResultListController(ResultListService resultListService)
        {
            _resultListService = resultListService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResultLists()
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
        public async Task<IActionResult> GetResultList(int id)
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

        [HttpPost("Create")]
        public async Task<IActionResult> CreateResultList(List<Result> resultList)
        {
            try
            {
                await _resultListService.CreateResultList(resultList);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}