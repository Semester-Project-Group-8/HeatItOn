using Microsoft.AspNetCore.Mvc;

namespace Backend.Interfaces
{
    public interface IController<TCreate, TUpdate>
    {
        Task<IActionResult> List();
        Task<IActionResult> Get(int id);
        Task<IActionResult> Post([FromBody] TCreate model);
        Task<IActionResult> Put(int id, [FromBody] TUpdate model);
        Task<IActionResult> Delete(int id);
    }
}