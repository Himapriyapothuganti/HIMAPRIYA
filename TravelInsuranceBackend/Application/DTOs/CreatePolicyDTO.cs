using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreatePolicyDTO : IValidatableObject
    {
        [Required]
        public int PolicyProductId { get; set; }

        [Required]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string TravellerName { get; set; } = string.Empty;

        [Required]
        [Range(1, 120, ErrorMessage = "Traveller age must be between 1 and 120.")]
        public int TravellerAge { get; set; }

        [Required]
        public string PassportNumber { get; set; } = string.Empty;


        // ── KYC ──────────────────────────────────────────
        [Required]
        public string KycType { get; set; } = string.Empty;  // PAN / Aadhaar / CKYC

        [Required]
        public string KycNumber { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.Date < DateTime.Today)
            {
                yield return new ValidationResult("Policy start date cannot be in the past.", new[] { nameof(StartDate) });
            }

            if (EndDate.Date <= StartDate.Date)
            {
                yield return new ValidationResult("Policy end date must be strictly after the start date.", new[] { nameof(EndDate) });
            }
        }
    }
}
