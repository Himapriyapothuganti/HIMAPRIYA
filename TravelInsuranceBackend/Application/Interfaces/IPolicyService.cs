using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPolicyService
    {
        // Customer
        Task<List<PolicyProductResponseDTO>> GetAvailablePolicyProductsAsync();
        Task<PolicyResponseDTO> PurchasePolicyAsync(string customerId, CreatePolicyDTO dto);
        Task<PolicyResponseDTO> PayRequestAsync(string customerId, PayPolicyRequestDTO dto);
        Task<PaymentResponseDTO> MakePaymentAsync(string customerId, PolicyPaymentDTO dto);
        Task<List<PolicyResponseDTO>> GetMyPoliciesAsync(string customerId);
        Task<PolicyResponseDTO> GetPolicyByIdAsync(int policyId, string customerId);

        // Admin
        Task<List<PolicyResponseDTO>> GetAllPoliciesAsync();

        // Recommendations
        Task<RecommendationResponseDTO> GetSmartRecommendationAsync(RecommendationRequestDTO request);

        // Invoices
        Task<InvoiceDTO> GetInvoiceAsync(int policyId, string customerId);
    }
}
