using System;

namespace Application.DTOs
{
    public class InvoiceDTO
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string TravellerName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal BasePremium { get; set; }
        public decimal TaxAmount { get; set; } // 18% GST typical in India
        public decimal TotalAmount { get; set; }
        
        public string PaymentMethod { get; set; } = "Online";
        public string CompanyName { get; set; } = "TalkTravel Insurance Corp";
        public string CompanyAddress { get; set; } = "123, Tech Park, Bangalore, KA, 560103";
        public string CompanyGSTIN { get; set; } = "29AAAAA0000A1Z5";
    }
}
