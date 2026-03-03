using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ClaimsOfficerService : IClaimsOfficerService
    {
        private readonly IClaimRepository _claimRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsOfficerService(
            IClaimRepository claimRepo,
            IPolicyRepository policyRepo,
            UserManager<ApplicationUser> userManager)
        {
            _claimRepo = claimRepo;
            _policyRepo = policyRepo;
            _userManager = userManager;
        }

        public async Task<OfficerDashboardDTO> GetDashboardAsync(string officerId)
        {
            var officer = await _userManager.FindByIdAsync(officerId)
                          ?? throw new Exception("Officer not found.");

            var claims = await _claimRepo.GetByOfficerIdAsync(officerId);

            var approvedClaims = claims
                .Where(c => c.Status == ClaimStatus.Approved ||
                            c.Status == ClaimStatus.PaymentProcessed ||
                            c.Status == ClaimStatus.Closed)
                .ToList();

            return new OfficerDashboardDTO
            {
                OfficerId = officerId,
                OfficerName = officer.FullName,
                Email = officer.Email!,
                TotalAssignedClaims = claims.Count,
                UnderReviewClaims = claims.Count(c => c.Status == ClaimStatus.UnderReview),
                PendingDocumentsClaims = claims.Count(c => c.Status == ClaimStatus.PendingDocuments),
                ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
                RejectedClaims = claims.Count(c => c.Status == ClaimStatus.Rejected),
                ClosedClaims = claims.Count(c => c.Status == ClaimStatus.Closed),
                TotalApprovedAmount = approvedClaims.Sum(c => c.ApprovedAmount ?? 0),
                AssignedClaims = await MapClaimsAsync(claims)
            };
        }

        private async Task<List<ClaimResponseDTO>> MapClaimsAsync(
            List<Domain.Entities.Claim> claims)
        {
            var result = new List<ClaimResponseDTO>();
            foreach (var claim in claims)
            {
                var customer = await _userManager.FindByIdAsync(claim.CustomerId);
                var officer = claim.ClaimsOfficerId != null
                               ? await _userManager.FindByIdAsync(claim.ClaimsOfficerId)
                               : null;
                var policy = await _policyRepo.GetByIdAsync(claim.PolicyId);

                result.Add(new ClaimResponseDTO
                {
                    ClaimId = claim.ClaimId,
                    PolicyId = claim.PolicyId,
                    PolicyNumber = policy.PolicyNumber,
                    CustomerId = claim.CustomerId,
                    CustomerName = customer?.FullName ?? "",
                    ClaimsOfficerId = claim.ClaimsOfficerId,
                    ClaimsOfficerName = officer?.FullName ?? "",
                    ClaimType = claim.ClaimType,
                    Description = claim.Description,
                    ClaimedAmount = claim.ClaimedAmount,
                    ApprovedAmount = claim.ApprovedAmount,
                    Status = claim.Status.ToString(),
                    RejectionReason = claim.RejectionReason,
                    SubmittedAt = claim.SubmittedAt,
                    ReviewedAt = claim.ReviewedAt
                });
            }
            return result;
        }
    }
}

