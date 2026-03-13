using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepo;
        private readonly IPolicyProductRepository _policyProductRepo;
        private readonly IUserRepository _userRepo;
        private readonly IClaimRepository _claimRepo;
        private readonly IPolicyRequestRepository _policyRequestRepo;

        public AdminService(
            UserManager<ApplicationUser> userManager,
            IPolicyProductRepository policyProductRepo,
            IPolicyRepository policyRepo,
            IUserRepository userRepo,
            IClaimRepository claimRepo,
            IPolicyRequestRepository policyRequestRepo)
        {
            _userManager = userManager;
            _policyProductRepo = policyProductRepo;
            _policyRepo = policyRepo;
            _userRepo = userRepo;
            _claimRepo = claimRepo;
            _policyRequestRepo = policyRequestRepo;
        }

        // ── CREATE AGENT / CLAIMS OFFICER ─────────────────
        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO dto)
        {
            if (dto.Role != UserRole.Agent && dto.Role != UserRole.ClaimsOfficer)
                throw new Exception("You can only create Agent or ClaimsOfficer accounts.");

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("A user with this email already exists.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, dto.Role);
            return MapToUserResponse(user, dto.Role);
        }

        // ── GET ALL USERS ─────────────────────────────────
        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            var result = new List<UserResponseDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? user.Role;
                var dto = MapToUserResponse(user, role);

                // For customers, resolve assigned agent from most recent policy request or policy
                if (role == UserRole.Customer)
                {
                    // First check policy requests (shows agent even before payment)
                    var requests = await _policyRequestRepo.GetByCustomerIdAsync(user.Id);
                    var latestRequest = requests.OrderByDescending(r => r.RequestedAt).FirstOrDefault();
                    if (latestRequest?.AgentId != null)
                    {
                        var agent = await _userManager.FindByIdAsync(latestRequest.AgentId);
                        dto.AssignedAgentName = agent?.FullName;
                    }
                    else
                    {
                        // Fallback to policies if no requests exist
                        var policies = await _policyRepo.GetByCustomerIdAsync(user.Id);
                        var latestPolicy = policies.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                        if (latestPolicy?.AgentId != null)
                        {
                            var agent = await _userManager.FindByIdAsync(latestPolicy.AgentId);
                            dto.AssignedAgentName = agent?.FullName;
                        }
                    }
                }

                result.Add(dto);
            }

            return result;
        }

        // ── ACTIVATE / DEACTIVATE USER ────────────────────
        public async Task<string> ActivateDeactivateUserAsync(string userId, bool isActive)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            user.IsActive = isActive;
            await _userRepo.UpdateAsync(user);
            return isActive ? "User activated successfully." : "User deactivated successfully.";
        }

        // ── CREATE POLICY PRODUCT ─────────────────────────
        public async Task<PolicyProductResponseDTO> CreatePolicyProductAsync(CreatePolicyProductDTO dto)
        {
            var product = new PolicyProduct
            {
                PolicyName = dto.PolicyName,
                PolicyType = dto.PolicyType,
                PlanTier = dto.PlanTier,
                CoverageDetails = dto.CoverageDetails,
                CoverageLimit = dto.CoverageLimit,
                BasePremium = dto.BasePremium,
                Tenure = dto.Tenure,
                ClaimLimit = dto.ClaimLimit,
                DestinationZone = dto.DestinationZone,
                Status = PolicyProductStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _policyProductRepo.CreateAsync(product);
            return MapToPolicyProductResponse(created);
        }

        // ── GET ALL POLICY PRODUCTS ───────────────────────
        public async Task<List<PolicyProductResponseDTO>> GetAllPolicyProductsAsync()
        {
            var products = await _policyProductRepo.GetAllAsync();
            return products.Select(MapToPolicyProductResponse).ToList();
        }

        // ── UPDATE POLICY PRODUCT ─────────────────────────
        public async Task<PolicyProductResponseDTO> UpdatePolicyProductAsync(int id, CreatePolicyProductDTO dto)
        {
            var product = await _policyProductRepo.GetByIdAsync(id);
            product.PolicyName = dto.PolicyName;
            product.PolicyType = dto.PolicyType;
            product.PlanTier = dto.PlanTier;
            product.CoverageDetails = dto.CoverageDetails;
            product.CoverageLimit = dto.CoverageLimit;
            product.BasePremium = dto.BasePremium;
            product.Tenure = dto.Tenure;
            product.ClaimLimit = dto.ClaimLimit;
            product.DestinationZone = dto.DestinationZone;

            var updated = await _policyProductRepo.UpdateAsync(product);
            return MapToPolicyProductResponse(updated);
        }

        // ── ACTIVATE / DEACTIVATE POLICY PRODUCT ─────────
        public async Task<string> ActivateDeactivatePolicyProductAsync(int id, bool isActive)
        {
            var product = await _policyProductRepo.GetByIdAsync(id);
            product.Status = isActive ? PolicyProductStatus.Available : PolicyProductStatus.Inactive;
            await _policyProductRepo.UpdateAsync(product);
            return isActive ? "Policy product activated." : "Policy product deactivated.";
        }

        // ── DELETE POLICY PRODUCT ─────────────────────────
        public async Task<string> DeletePolicyProductAsync(int id)
        {
            var isUsed = await _policyProductRepo.ExistsInPoliciesAsync(id);
            if (isUsed)
                throw new Exception("Cannot delete. Policy product is already in use.");

            await _policyProductRepo.DeleteAsync(id);
            return "Policy product deleted successfully.";
        }

        // ── ASSIGN AGENT TO POLICY ────────────────────────
        public async Task<PolicyResponseDTO> AssignAgentToPolicyAsync(AssignAgentDTO dto)
        {
            // Validate policy exists
            var policy = await _policyRepo.GetByIdAsync(dto.PolicyId);

            // Validate agent exists and is active
            var agent = await _userRepo.GetByIdAsync(dto.AgentId);
            if (!agent.IsActive)
                throw new Exception("Agent is not active.");

            // Check agent role
            var roles = await _userManager.GetRolesAsync(agent);
            if (!roles.Contains(UserRole.Agent))
                throw new Exception("Selected user is not an Agent.");

            // Assign agent permanently
            policy.AgentId = dto.AgentId;
            await _policyRepo.UpdateAsync(policy);

            return await MapToPolicyResponse(policy);
        }

        // ── MAPPERS ───────────────────────────────────────
        private static UserResponseDTO MapToUserResponse(ApplicationUser user, string role) =>
            new()
            {
                Id = user.Id,
                FullName = user.FullName ?? "Unknown User",
                Email = user.Email ?? "No Email",
                Role = role ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

        private static PolicyProductResponseDTO MapToPolicyProductResponse(PolicyProduct p) =>
            new()
            {
                PolicyProductId = p.PolicyProductId,
                PolicyName = p.PolicyName,
                PolicyType = p.PolicyType,
                PlanTier = p.PlanTier,
                CoverageDetails = p.CoverageDetails,
                ExclusionDetails = p.ExclusionDetails,
                CoverageLimit = p.CoverageLimit,
                BasePremium = p.BasePremium,
                Tenure = p.Tenure,
                ClaimLimit = p.ClaimLimit,
                DestinationZone = p.DestinationZone,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            };

        private async Task<PolicyResponseDTO> MapToPolicyResponse(Policy policy)
        {
            var customer = await _userManager.FindByIdAsync(policy.CustomerId);
            var agent = policy.AgentId != null
                           ? await _userManager.FindByIdAsync(policy.AgentId)
                           : null;
            var product = await _policyProductRepo.GetByIdAsync(policy.PolicyProductId);

            return new PolicyResponseDTO
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                CustomerId = policy.CustomerId,
                CustomerName = customer?.FullName ?? "",
                AgentId = policy.AgentId,
                AgentName = agent?.FullName ?? "",
                PolicyProductId = policy.PolicyProductId,
                PolicyName = product.PolicyName,
                PolicyType = policy.PolicyType,
                CoverageDetails = product.CoverageDetails ?? "N/A",
                PlanTier = policy.PlanTier,
                Destination = policy.Destination,
                TravellerName = policy.TravellerName,
                TravellerAge = policy.TravellerAge,
                PassportNumber = policy.PassportNumber,
                KycType = policy.KycType,
                KycNumber = policy.KycNumber,

                PremiumAmount = policy.PremiumAmount,
                CoverageLimit = product.CoverageLimit,
                ClaimLimit = product.ClaimLimit,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                Status = policy.Status.ToString(),
                CreatedAt = policy.CreatedAt
            };
        }

        public async Task<AdminDashboardDTO> GetDashboardAsync()
        {
            // Users
            var allUsers = await _userRepo.GetAllAsync();
            var customers = await _userRepo.GetByRoleAsync(UserRole.Customer);
            var agents = await _userRepo.GetByRoleAsync(UserRole.Agent);
            var officers = await _userRepo.GetByRoleAsync(UserRole.ClaimsOfficer);

            // Policies
            var policies = await _policyRepo.GetAllAsync();
            var activePolicies = policies.Where(p => p.Status == PolicyStatus.Active).ToList();

            // Claims
            var claims = await _claimRepo.GetAllAsync();

            // Agent Performance
            var agentPerformance = new List<AgentPerformanceDTO>();
            foreach (var agent in agents)
            {
                var agentPolicies = policies.Where(p => p.AgentId == agent.Id).ToList();
                var premium = agentPolicies
                    .Where(p => p.Status == PolicyStatus.Active)
                    .Sum(p => p.PremiumAmount);

                agentPerformance.Add(new AgentPerformanceDTO
                {
                    AgentId = agent.Id,
                    AgentName = agent.FullName ?? "Unknown Agent",
                    Email = agent.Email ?? "No Email",
                    TotalPolicies = agentPolicies.Count,
                    ActivePolicies = agentPolicies.Count(p => p.Status == PolicyStatus.Active),
                    TotalPremiumCollected = premium,
                    CommissionEarned = Math.Round(premium * 0.05m, 2)
                });
            }

            return new AdminDashboardDTO
            {
                // Users
                TotalUsers = allUsers.Count,
                TotalCustomers = customers.Count,
                TotalAgents = agents.Count,
                TotalClaimsOfficers = officers.Count,

                // Policies
                TotalPolicies = policies.Count,
                ActivePolicies = policies.Count(p => p.Status == PolicyStatus.Active),
                PendingPaymentPolicies = policies.Count(p => p.Status == PolicyStatus.PendingPayment),
                ExpiredPolicies = policies.Count(p => p.Status == PolicyStatus.Expired),
                TotalPremiumRevenue = activePolicies.Sum(p => p.PremiumAmount),

                // Claims — all 7 statuses counted correctly
                TotalClaims = claims.Count,
                SubmittedClaims = claims.Count(c => c.Status == ClaimStatus.Submitted),
                UnderReviewClaims = claims.Count(c => c.Status == ClaimStatus.UnderReview),
                PendingDocumentsClaims = claims.Count(c => c.Status == ClaimStatus.PendingDocuments),
                ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
                RejectedClaims = claims.Count(c => c.Status == ClaimStatus.Rejected),
                PaymentProcessedClaims = claims.Count(c => c.Status == ClaimStatus.PaymentProcessed),
                ClosedClaims = claims.Count(c => c.Status == ClaimStatus.Closed),
                TotalClaimedAmount = claims.Sum(c => c.ClaimedAmount),
                TotalApprovedAmount = claims.Sum(c => c.ApprovedAmount ?? 0),

                AgentPerformance = agentPerformance
            };
        }
    }
}