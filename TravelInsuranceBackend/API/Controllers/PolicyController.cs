using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // ── PUBLIC - Browse available products ────────────
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableProducts()
        {
            var result = await _policyService.GetAvailablePolicyProductsAsync();
            return Ok(result);
        }

        // ── CUSTOMER - Purchase policy ────────────────────
        [HttpPost("purchase")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> PurchasePolicy([FromBody] CreatePolicyDTO dto)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.PurchasePolicyAsync(customerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── CUSTOMER - Make payment ───────────────────────
        [HttpPost("pay")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> MakePayment([FromBody] PolicyPaymentDTO dto)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.MakePaymentAsync(customerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── CUSTOMER - My policies ────────────────────────
        [HttpGet("my-policies")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetMyPolicies()
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.GetMyPoliciesAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── CUSTOMER - Get policy by id ───────────────────
        [HttpGet("{policyId}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetPolicyById(int policyId)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.GetPolicyByIdAsync(policyId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── ADMIN - Get all policies ──────────────────────
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        // ── CUSTOMER - Pay for Approved Request ───────────
        [HttpPost("pay-request")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> PayRequest([FromBody] PayPolicyRequestDTO dto)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.PayRequestAsync(customerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PUBLIC - Smart Recommendation ─────────────────
        [HttpPost("recommend")]
        [AllowAnonymous]
        public async Task<IActionResult> RecommendPlan([FromBody] RecommendationRequestDTO dto)
        {
            try
            {
                var result = await _policyService.GetSmartRecommendationAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── CUSTOMER - Get Invoice ───────────────────────
        [HttpGet("{policyId}/invoice")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetInvoice(int policyId)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _policyService.GetInvoiceAsync(policyId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
