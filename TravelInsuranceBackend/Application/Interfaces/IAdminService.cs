using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminService
    {
        // User Management
        Task<UserResponseDTO> CreateUserAsync(CreateUserDTO dto);
        Task<List<UserResponseDTO>> GetAllUsersAsync();
        Task<string> ActivateDeactivateUserAsync(string userId, bool isActive);

        // Policy Product Management
        Task<PolicyProductResponseDTO> CreatePolicyProductAsync(CreatePolicyProductDTO dto);
        Task<List<PolicyProductResponseDTO>> GetAllPolicyProductsAsync();
        Task<PolicyProductResponseDTO> UpdatePolicyProductAsync(int id, CreatePolicyProductDTO dto);
        Task<string> ActivateDeactivatePolicyProductAsync(int id, bool isActive);
        Task<string> DeletePolicyProductAsync(int id);
        
        // Agent Assignment
        Task<PolicyResponseDTO> AssignAgentToPolicyAsync(AssignAgentDTO dto);
        

        Task<AdminDashboardDTO> GetDashboardAsync();
    }
}
