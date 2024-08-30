using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MesaProject.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimManagementController : ControllerBase
    {
        public ClaimManagementController()
        {
            
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(HttpContext.User.Claims.ToList());
        }

    }
}
