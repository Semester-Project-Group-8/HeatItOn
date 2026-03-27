using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("Optimize")]
    [ApiController]
    public class OptimizerController : ControllerBase
    {
        private readonly OptimizerService _optimizerService;

        public OptimizerController(OptimizerService optimizerService)
        {
            _optimizerService = optimizerService;
        }
    }
}
