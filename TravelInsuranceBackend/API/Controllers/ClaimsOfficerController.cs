using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsOfficerController : ControllerBase
    {
        private readonly IClaimsOfficerService _officerService;

        public ClaimsOfficerController(IClaimsOfficerService officerService)
        {
            _officerService = officerService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("Officer not found.");
                var result = await _officerService.GetDashboardAsync(officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

