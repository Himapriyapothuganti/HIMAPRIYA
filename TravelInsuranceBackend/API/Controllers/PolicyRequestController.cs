using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PolicyRequestController : ControllerBase
    {
        private readonly IPolicyRequestService _requestService;

        public PolicyRequestController(IPolicyRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SubmitRequest([FromForm] CreatePolicyRequestDTO dto, IFormFile kycFile, IFormFile passportFile, IFormFile? otherFile)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerId)) return Unauthorized("User not found.");

                var result = await _requestService.SubmitRequestAsync(customerId, dto, kycFile, passportFile, otherFile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateRequest(int id, [FromForm] CreatePolicyRequestDTO dto, IFormFile? kycFile, IFormFile? passportFile, IFormFile? otherFile)
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerId)) return Unauthorized("User not found.");

                var result = await _requestService.UpdateRequestAsync(id, customerId, dto, kycFile, passportFile, otherFile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyRequests()
        {
            try
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerId)) return Unauthorized("User not found.");

                var result = await _requestService.GetMyRequestsAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                // Both Agents and Customers can download
                var (fileBytes, contentType, fileName) = await _requestService.DownloadDocumentAsync(documentId);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
