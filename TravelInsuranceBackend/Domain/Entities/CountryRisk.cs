using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class CountryRisk
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public decimal Multiplier { get; set; } = 1.0m;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
