using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // ── CUSTOMER ──────────────────────────────────────

        // This endpoint is called by Angular's customer.service.ts
        // Full URL: POST https://localhost:7161/api/Claim/submit
        [HttpPost("submit")]
        // Security: Only logged-in users with role "Customer" can call this
        // Anyone without a valid JWT token gets 401 Unauthorized automatically
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        // Tells the API to expect file uploads + form fields together
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitClaim([FromForm] CreateClaimDTO dto)
        {
            try
            {
                // Read the logged-in customer's ID from the JWT token
                // (Angular sends this token automatically in the request header)
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");

                // Hand off to ClaimService for business logic
                var result = await _claimService.SubmitClaimAsync(customerId, dto);

                //Success — return 200 OK with the saved claim details
                return Ok(result);
            }
            catch (Exception ex)
            {
                //Failure — return 400 Bad Request with the error message
                // This message is what shows up in the Angular UI
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-claims")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetMyClaims()
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _claimService.GetMyClaimsAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{claimId}")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetClaimById(int claimId)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? throw new Exception("User not found.");
                var result = await _claimService.GetClaimByIdAsync(claimId, customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── CLAIMS OFFICER ────────────────────────────────

        [HttpGet("assigned")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> GetAssignedClaims()
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.GetAssignedClaimsAsync(officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{claimId}/review")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> ReviewClaim(int claimId, [FromBody] ReviewClaimDTO dto)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.ReviewClaimAsync(claimId, officerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{claimId}/request-documents")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> RequestDocuments(int claimId, [FromBody] RequestDocumentDTO dto)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.RequestDocumentsAsync(claimId, officerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{claimId}/process-payment")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> ProcessPayment(int claimId)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.ProcessPaymentAsync(claimId, officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{claimId}/close")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> CloseClaim(int claimId)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.CloseClaimAsync(claimId, officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{claimId}/analyze")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> AnalyzeClaim(int claimId)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? throw new Exception("User not found.");
                var result = await _claimService.AnalyzeClaimAsync(claimId, officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── SHARED / DOCUMENT DOWNLOAD ────────────────────

        [HttpGet("document/{documentId}")]
        [Authorize] // Allow Customer, ClaimsOfficer, or Admin if they have a valid token
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var (fileBytes, contentType, fileName) = await _claimService.DownloadDocumentAsync(documentId);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── ADMIN ─────────────────────────────────────────

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClaims()
        {
            try
            {
                var result = await _claimService.GetAllClaimsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}