using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IClaimService
    {
        // Customer
        Task<ClaimResponseDTO> SubmitClaimAsync(string customerId, CreateClaimDTO dto);
        Task<List<ClaimResponseDTO>> GetMyClaimsAsync(string customerId);
        Task<ClaimResponseDTO> GetClaimByIdAsync(int claimId, string customerId);

        // Claims Officer
        Task<List<ClaimResponseDTO>> GetAssignedClaimsAsync(string officerId);
        Task<ClaimResponseDTO> ReviewClaimAsync(int claimId, string officerId, ReviewClaimDTO dto);
        Task<ClaimResponseDTO> RequestDocumentsAsync(int claimId, string officerId, RequestDocumentDTO dto);
        Task<ClaimResponseDTO> ProcessPaymentAsync(int claimId, string officerId);
        Task<ClaimResponseDTO> CloseClaimAsync(int claimId, string officerId);

        // Admin
        Task<List<ClaimResponseDTO>> GetAllClaimsAsync();

        // Shared/Document Download
        Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadDocumentAsync(int documentId);
    }
}
