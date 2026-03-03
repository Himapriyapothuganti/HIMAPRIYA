using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // Only Admin can access
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ── USER MANAGEMENT ───────────────────────────────

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            try
            {
                var result = await _adminService.CreateUserAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _adminService.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> ActivateDeactivateUser(string userId, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _adminService.ActivateDeactivateUserAsync(userId, isActive);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── POLICY PRODUCT MANAGEMENT ─────────────────────

        [HttpPost("policy-products")]
        public async Task<IActionResult> CreatePolicyProduct([FromBody] CreatePolicyProductDTO dto)
        {
            try
            {
                var result = await _adminService.CreatePolicyProductAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("policy-products")]
        public async Task<IActionResult> GetAllPolicyProducts()
        {
            var result = await _adminService.GetAllPolicyProductsAsync();
            return Ok(result);
        }

        [HttpPut("policy-products/{id}")]
        public async Task<IActionResult> UpdatePolicyProduct(int id, [FromBody] CreatePolicyProductDTO dto)
        {
            try
            {
                var result = await _adminService.UpdatePolicyProductAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("policy-products/{id}/status")]
        public async Task<IActionResult> ActivateDeactivatePolicyProduct(int id, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _adminService.ActivateDeactivatePolicyProductAsync(id, isActive);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("assign-agent")]
        public async Task<IActionResult> AssignAgentToPolicy([FromBody] AssignAgentDTO dto)
        {
            try
            {
                var result = await _adminService.AssignAgentToPolicyAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("policy-products/{id}")]
        public async Task<IActionResult> DeletePolicyProduct(int id)
        {
            try
            {
                var result = await _adminService.DeletePolicyProductAsync(id);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var result = await _adminService.GetDashboardAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

