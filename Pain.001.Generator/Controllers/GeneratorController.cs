using Microsoft.AspNetCore.Mvc;
using Pain._001.Generator.Services;

namespace Pain._001.Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        [HttpGet("{payments}")]
        public ActionResult<string> GeneratePaymentFile([FromRoute] int payments)
        {
            return Ok(GeneratorService.GenerateFile(payments));
        }
    }
}
