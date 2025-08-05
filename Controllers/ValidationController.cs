using CEA_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CEA_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        public ValidationController(J54hfncyh4CeaContext DB_CEA_Context)
        {
            _DB_CEA_Context = DB_CEA_Context;
        }

        [HttpGet]
        [Route("Validate")]
        public async Task<IActionResult> Validate()
        {
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
        }
    }
}
