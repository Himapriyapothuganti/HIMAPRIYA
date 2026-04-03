using System;

namespace Domain.Entities
{
    public class PolicyRequestDocument
    {
        public int PolicyRequestDocumentId { get; set; }
        public int PolicyRequestId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // KYC / Passport / Other
        public long FileSize { get; set; }
        public bool IsLatest { get; set; } = true; // For tracking replaced versions
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public PolicyRequest? PolicyRequest { get; set; }
    }
}
