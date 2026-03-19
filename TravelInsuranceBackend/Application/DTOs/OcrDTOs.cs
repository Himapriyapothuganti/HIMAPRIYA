using System;

namespace Application.DTOs
{
    public class ExtractedDataDTO
    {
        public string? FullName { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Confidence { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
