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
        private readonly IPolicyProductRepository _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsOfficerService(
            IClaimRepository claimRepo,
            IPolicyRepository policyRepo,
            IPolicyProductRepository productRepo,
            UserManager<ApplicationUser> userManager)
        {
            _claimRepo = claimRepo;
            _policyRepo = policyRepo;
            _productRepo = productRepo;
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
                var product = await _productRepo.GetByIdAsync(policy.PolicyProductId);

                var payoutCalc = CalculateSuggestedPayout(claim, product);

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
                    SuggestedPayout = payoutCalc.SuggestedPayout,
                    DeductibleApplied = payoutCalc.DeductibleApplied,
                    SubmittedAt = claim.SubmittedAt,
                    ReviewedAt = claim.ReviewedAt
                });
            }
            return result;
        }

        private (decimal SuggestedPayout, decimal DeductibleApplied) CalculateSuggestedPayout(Domain.Entities.Claim claim, PolicyProduct product)
        {
            var claimType = claim.ClaimType;
            var requested = claim.ClaimedAmount;

            var result = claimType switch
            {
                "Emergency Medical" => (Math.Max(0, Math.Min(requested, product.CoverageLimit) - 8300), 8300m),
                "Dental" => (Math.Max(0, Math.Min(requested, 24900m) - 12450m), 12450m),
                "Hospital Cash" => (Math.Min(requested, 6225m), 0m),
                "Personal Accident" => (Math.Min(requested, 415000m), 0m),
                "Baggage Loss" => (Math.Min(requested, 16600m), 0m),
                "Baggage Delay" => (Math.Min(requested, 20750m), 0m),
                "Flight Cancellation" => (Math.Min(requested, 8300m), 0m),
                "Trip Cancellation" => (Math.Max(0, Math.Min(requested, 8300m) - 4150m), 4150m),
                "Loss of Passport" => (Math.Min(requested, 16600m), 0m),
                "Flight Delay" => (Math.Min(requested, 8300m), 0m),
                "Emergency Hotel" => (Math.Max(0, Math.Min(requested, 83000m) - 8300m), 8300m),
                "Personal Liability" => (Math.Min(requested, 830000m), 0m),
                "Missed Connection" => (Math.Min(requested, 41500m), 0m),
                "Emergency Cash" => (Math.Min(requested, 41500m), 0m),
                "Pre-existing Disease" => (Math.Max(0, Math.Min(requested, product.CoverageLimit * 0.03m) - 8300m), 8300m),
                "Hijack Distress" => (Math.Min(requested, 8300m), 0m),
                _ => (Math.Min(requested, product.ClaimLimit), 0m)
            };

            return result;
        }
    }
}

