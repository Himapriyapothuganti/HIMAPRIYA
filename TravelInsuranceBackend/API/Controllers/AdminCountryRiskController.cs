using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/admin/country-risks")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCountryRiskController : ControllerBase
    {
        private readonly ICountryRiskService _countryRiskService;

        public AdminCountryRiskController(ICountryRiskService countryRiskService)
        {
            _countryRiskService = countryRiskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _countryRiskService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous] // Allow customer portal to see allowed countries
        public async Task<IActionResult> GetActive()
        {
            var result = await _countryRiskService.GetActiveAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _countryRiskService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCountryRiskDTO dto)
        {
            try
            {
                var result = await _countryRiskService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Unique") == true || ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    return BadRequest(new { message = $"A country with the name '{dto.Name}' already exists." });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCountryRiskDTO dto)
        {
            try
            {
                await _countryRiskService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _countryRiskService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
