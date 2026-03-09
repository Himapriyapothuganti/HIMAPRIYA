using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPolicyRequestService
    {
        Task<PolicyRequestResponseDTO> SubmitRequestAsync(string customerId, CreatePolicyRequestDTO dto, Microsoft.AspNetCore.Http.IFormFile kycFile, Microsoft.AspNetCore.Http.IFormFile passportFile, Microsoft.AspNetCore.Http.IFormFile? otherFile);
        Task<List<PolicyRequestResponseDTO>> GetMyRequestsAsync(string customerId);
        Task<List<AgentPolicyRequestResponseDTO>> GetAgentRequestsAsync(string agentId);
        Task<AgentPolicyRequestResponseDTO> GetRequestByIdAsync(int requestId, string userId, bool isAgent);
        Task<AgentPolicyRequestResponseDTO> ReviewRequestAsync(int requestId, string agentId, ReviewPolicyRequestDTO dto);
        Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadDocumentAsync(int documentId);
    }
}
