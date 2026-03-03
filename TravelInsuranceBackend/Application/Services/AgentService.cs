using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IPolicyRepository _policyRepo;
        private readonly IPolicyProductRepository _productRepo;
        private readonly IClaimRepository _claimRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        private const decimal CommissionRate = 0.05m;

        public AgentService(
            IPolicyRepository policyRepo,
            IPolicyProductRepository productRepo,
            IClaimRepository claimRepo,   // ← fixed
            UserManager<ApplicationUser> userManager)
        {
            _policyRepo = policyRepo;
            _productRepo = productRepo;
            _claimRepo = claimRepo;   // ← fixed
            _userManager = userManager;
        }

        // ── GET DASHBOARD ─────────────────────────────────
        public async Task<AgentDashboardDTO> GetDashboardAsync(string agentId)
        {
            var agent = await _userManager.FindByIdAsync(agentId)
                        ?? throw new Exception("Agent not found.");
            var policies = await _policyRepo.GetByAgentIdAsync(agentId);

            var activePolicies = policies
                .Where(p => p.Status == PolicyStatus.Active)
                .ToList();

            var totalPremium = activePolicies.Sum(p => p.PremiumAmount);
            var totalCommission = totalPremium * CommissionRate;

            return new AgentDashboardDTO
            {
                AgentId = agentId,
                AgentName = agent.FullName,
                Email = agent.Email!,
                TotalPoliciesAssigned = policies.Count,
                ActivePolicies = policies.Count(p => p.Status == PolicyStatus.Active),
                PendingPaymentPolicies = policies.Count(p => p.Status == PolicyStatus.PendingPayment),
                ExpiredPolicies = policies.Count(p => p.Status == PolicyStatus.Expired),
                TotalPremiumCollected = totalPremium,
                TotalCommissionEarned = Math.Round(totalCommission, 2),
                AssignedPolicies = await MapPoliciesAsync(policies)
            };
        }

        // ── GET ASSIGNED POLICIES ─────────────────────────
        public async Task<List<PolicyResponseDTO>> GetAssignedPoliciesAsync(string agentId)
        {
            var policies = await _policyRepo.GetByAgentIdAsync(agentId);
            return await MapPoliciesAsync(policies);
        }

        // ── GET POLICY DETAIL ─────────────────────────────
        public async Task<PolicyResponseDTO> GetPolicyDetailAsync(string agentId, int policyId)
        {
            var policy = await _policyRepo.GetByIdAsync(policyId);

            if (policy.AgentId != agentId)
                throw new Exception("You are not assigned to this policy.");

            return await MapPolicyAsync(policy, includeClaims: true);
        }

        // ── MAPPERS ───────────────────────────────────────
        private async Task<List<PolicyResponseDTO>> MapPoliciesAsync(
            List<Domain.Entities.Policy> policies)
        {
            var result = new List<PolicyResponseDTO>();
            foreach (var p in policies)
                result.Add(await MapPolicyAsync(p));
            return result;
        }

        private async Task<PolicyResponseDTO> MapPolicyAsync(
            Domain.Entities.Policy policy,
            bool includeClaims = false)   // ← fixed
        {
            var customer = await _userManager.FindByIdAsync(policy.CustomerId);
            var agent = policy.AgentId != null
                           ? await _userManager.FindByIdAsync(policy.AgentId)
                           : null;
            var product = await _productRepo.GetByIdAsync(policy.PolicyProductId);

            // ── Claims (only for detail view) ─────────────
            var claims = new List<ClaimSummaryDTO>();
            if (includeClaims)
            {
                var policyClaims = await _claimRepo.GetByPolicyIdAsync(policy.PolicyId);
                claims = policyClaims.Select(c => new ClaimSummaryDTO
                {
                    ClaimId = c.ClaimId,
                    ClaimType = c.ClaimType,
                    ClaimedAmount = c.ClaimedAmount,
                    Status = c.Status.ToString(),
                    SubmittedAt = c.SubmittedAt
                }).ToList();
            }

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
                PlanTier = policy.PlanTier,
                Destination = policy.Destination,
                TravellerName = policy.TravellerName,
                TravellerAge = policy.TravellerAge,
                PassportNumber = policy.PassportNumber,

                // ── KYC ──────────────────────────────────
                KycType = policy.KycType,
                KycNumber = policy.KycNumber,

                PremiumAmount = policy.PremiumAmount,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                Status = policy.Status.ToString(),
                CreatedAt = policy.CreatedAt,

                // ── Claims ────────────────────────────────
                Claims = claims
            };
        }
    }
}