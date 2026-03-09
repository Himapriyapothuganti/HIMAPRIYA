using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateClaimDTO
    {
        [Required]
        public int PolicyId { get; set; }

        [Required]
        public string ClaimType { get; set; } = string.Empty;
        // Medical / Cancellation / Baggage / FlightDelay / Evacuation

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal ClaimedAmount { get; set; }

        // ── Supporting Documents ──────────────────────────
      
        public List<IFormFile>? Documents { get; set; } = new List<IFormFile>();

        [Required]
        public DateTime IncidentDate { get; set; }
    }
}