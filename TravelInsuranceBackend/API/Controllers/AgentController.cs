using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Agent")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        // ── GET DASHBOARD ─────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? throw new Exception("Agent not found.");
                var result = await _agentService.GetDashboardAsync(agentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET ASSIGNED POLICIES ─────────────────────────
        [HttpGet("policies")]
        public async Task<IActionResult> GetAssignedPolicies()
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? throw new Exception("Agent not found.");
                var result = await _agentService.GetAssignedPoliciesAsync(agentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET POLICY DETAIL ─────────────────────────────
        [HttpGet("policies/{policyId}")]
        public async Task<IActionResult> GetPolicyDetail(int policyId)
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? throw new Exception("Agent not found.");
                var result = await _agentService.GetPolicyDetailAsync(agentId, policyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET POLICY REQUESTS ───────────────────────────
        [HttpGet("policy-requests")]
        public async Task<IActionResult> GetPolicyRequests([FromServices] IPolicyRequestService requestService)
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Agent not found.");
                var result = await requestService.GetAgentRequestsAsync(agentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET POLICY REQUEST DETAIL ─────────────────────
        [HttpGet("policy-requests/{requestId}")]
        public async Task<IActionResult> GetPolicyRequestDetail(int requestId, [FromServices] IPolicyRequestService requestService)
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Agent not found.");
                var result = await requestService.GetRequestByIdAsync(requestId, agentId, isAgent: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── REVIEW POLICY REQUEST ─────────────────────────
        [HttpPut("policy-requests/{requestId}/review")]
        public async Task<IActionResult> ReviewPolicyRequest(int requestId, [FromBody] Application.DTOs.ReviewPolicyRequestDTO dto, [FromServices] IPolicyRequestService requestService)
        {
            try
            {
                var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Agent not found.");
                var result = await requestService.ReviewRequestAsync(requestId, agentId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
