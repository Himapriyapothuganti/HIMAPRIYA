using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyProductRepository _productRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRequestRepository _requestRepo;

        public PolicyService(
            IPolicyProductRepository productRepo,
            IPolicyRepository policyRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager,
            IPolicyRequestRepository requestRepo)
        {
            _productRepo = productRepo;
            _policyRepo = policyRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _requestRepo = requestRepo;
        }

        // ── GET AVAILABLE POLICY PRODUCTS ─────────────────
        public async Task<List<PolicyProductResponseDTO>> GetAvailablePolicyProductsAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return products
                .Where(p => p.Status == PolicyProductStatus.Available)
                .Select(p => new PolicyProductResponseDTO
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
                }).ToList();
        }

        // ── PURCHASE POLICY ───────────────────────────────
        public async Task<PolicyResponseDTO> PurchasePolicyAsync(string customerId, CreatePolicyDTO dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.PolicyProductId);
            if (product.Status != PolicyProductStatus.Available)
                throw new Exception("This policy product is not available.");

            var existingPolicies = await _policyRepo.GetByCustomerIdAsync(customerId);
            bool hasDuplicate = existingPolicies.Any(p =>
                p.PolicyProductId == dto.PolicyProductId &&
                p.Status == PolicyStatus.Active);
            if (hasDuplicate)
                throw new Exception("You already have an active policy for this plan.");

            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new Exception("Start date cannot be in the past.");
            if (dto.EndDate <= dto.StartDate)
                throw new Exception("End date must be after start date.");

            var days = (dto.EndDate - dto.StartDate).Days;
            if (days > product.Tenure)
                throw new Exception($"Trip duration exceeds plan limit of {product.Tenure} days.");

            if (string.IsNullOrWhiteSpace(dto.KycType) || string.IsNullOrWhiteSpace(dto.KycNumber))
                throw new Exception("KYC details are required.");

            var premiumAmount = CalculatePremium(product.BasePremium, days, dto.TravellerAge);

            var policyNumber = $"POL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var policy = new Policy
            {
                PolicyProductId = dto.PolicyProductId,
                PolicyNumber = policyNumber,
                CustomerId = customerId,
                AgentId = null,
                Destination = dto.Destination,
                PolicyType = product.PolicyType,
                PlanTier = product.PlanTier,
                TravellerName = dto.TravellerName,
                TravellerAge = dto.TravellerAge,
                PassportNumber = dto.PassportNumber,
                KycType = dto.KycType,
                KycNumber = dto.KycNumber,
                PremiumAmount = premiumAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = PolicyStatus.PendingPayment,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _policyRepo.CreateAsync(policy);
            return await MapToPolicyResponse(created);
        }

        // ── PAY FOR APPROVED REQUEST ──────────────────────
        public async Task<PolicyResponseDTO> PayRequestAsync(string customerId, PayPolicyRequestDTO dto)
        {
            var request = await _requestRepo.GetByIdAsync(dto.PolicyRequestId);
            if (request.CustomerId != customerId)
                throw new Exception("You are not authorized for this request.");

            if (request.Status != "Approved")
                throw new Exception("Only approved requests can be purchased.");

            var product = await _productRepo.GetByIdAsync(request.PolicyProductId);
            var policyNumber = $"POL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var policy = new Policy
            {
                PolicyProductId = request.PolicyProductId,
                PolicyNumber = policyNumber,
                CustomerId = customerId,
                AgentId = request.AgentId,
                Destination = request.Destination,
                PolicyType = product.PolicyType,
                PlanTier = product.PlanTier,
                TravellerName = request.TravellerName,
                TravellerAge = request.TravellerAge,
                PassportNumber = request.PassportNumber,
                KycType = request.KycType,
                KycNumber = request.KycNumber,
                PremiumAmount = request.CalculatedPremium,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = PolicyStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var createdPolicy = await _policyRepo.CreateAsync(policy);
            request.Status = "Purchased";
            await _requestRepo.UpdateAsync(request);

            return await MapToPolicyResponse(createdPolicy);
        }

        // ── MAKE PAYMENT ──────────────────────────────────
        public async Task<PaymentResponseDTO> MakePaymentAsync(string customerId, PolicyPaymentDTO dto)
        {
            var policy = await _policyRepo.GetByIdAsync(dto.PolicyId);
            if (policy.CustomerId != customerId)
                throw new Exception("You are not authorized to pay for this policy.");

            if (policy.Status != PolicyStatus.PendingPayment)
                throw new Exception("This policy is not pending payment.");

            policy.Status = PolicyStatus.Active;
            await _policyRepo.UpdateAsync(policy);

            return new PaymentResponseDTO
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                AmountPaid = policy.PremiumAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = "Success",
                PaidAt = DateTime.UtcNow
            };
        }

        // ── GET MY POLICIES ───────────────────────────────
        public async Task<List<PolicyResponseDTO>> GetMyPoliciesAsync(string customerId)
        {
            var policies = await _policyRepo.GetByCustomerIdAsync(customerId);
            var result = new List<PolicyResponseDTO>();
            foreach (var p in policies)
                result.Add(await MapToPolicyResponse(p));
            return result;
        }

        // ── GET POLICY BY ID ──────────────────────────────
        public async Task<PolicyResponseDTO> GetPolicyByIdAsync(int policyId, string customerId)
        {
            var policy = await _policyRepo.GetByIdAsync(policyId);
            if (policy.CustomerId != customerId)
                throw new Exception("You are not authorized to view this policy.");
            return await MapToPolicyResponse(policy);
        }

        // ── GET ALL POLICIES (Admin) ──────────────────────
        public async Task<List<PolicyResponseDTO>> GetAllPoliciesAsync()
        {
            var policies = await _policyRepo.GetAllAsync();
            var result = new List<PolicyResponseDTO>();
            foreach (var p in policies)
                result.Add(await MapToPolicyResponse(p));
            return result;
        }

        // ── GET INVOICE ──────────────────────────────────
        public async Task<InvoiceDTO> GetInvoiceAsync(int policyId, string customerId)
        {
            var policy = await _policyRepo.GetByIdAsync(policyId);
            if (policy.CustomerId != customerId)
                throw new Exception("You are not authorized to view this invoice.");

            var product = await _productRepo.GetByIdAsync(policy.PolicyProductId);
            var customer = await _userManager.FindByIdAsync(policy.CustomerId);

            decimal basePremium = Math.Round(policy.PremiumAmount / 1.18m, 2);
            decimal taxAmount = policy.PremiumAmount - basePremium;

            return new InvoiceDTO
            {
                InvoiceNumber = $"INV-{policy.CreatedAt:yyyyMMdd}-{policy.PolicyId:D5}",
                PurchaseDate = policy.CreatedAt,
                PolicyNumber = policy.PolicyNumber,
                PolicyName = product.PolicyName,
                CustomerName = customer?.FullName ?? "Valued Customer",
                TravellerName = policy.TravellerName,
                Destination = policy.Destination,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                BasePremium = basePremium,
                TaxAmount = taxAmount,
                TotalAmount = policy.PremiumAmount,
                PaymentMethod = "Credit Card / Online"
            };
        }

        // ── PREMIUM CALCULATION ───────────────────────────
        private static decimal CalculatePremium(decimal basePremium, int days, int age)
        {
            var premium = basePremium * (days / 30m);
            if (age > 60) premium *= 1.3m;
            else if (age > 40) premium *= 1.1m;
            return Math.Round(premium, 2);
        }

        // ── MAPPER ────────────────────────────────────────
        private async Task<PolicyResponseDTO> MapToPolicyResponse(Policy policy)
        {
            var customer = await _userManager.FindByIdAsync(policy.CustomerId);
            var agent = policy.AgentId != null ? await _userManager.FindByIdAsync(policy.AgentId) : null;
            var product = await _productRepo.GetByIdAsync(policy.PolicyProductId);

            return new PolicyResponseDTO
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                CustomerId = policy.CustomerId,
                CustomerName = customer?.FullName ?? "Unknown Customer",
                AgentId = policy.AgentId,
                AgentName = agent?.FullName ?? "Unassigned",
                PolicyProductId = policy.PolicyProductId,
                PolicyName = product?.PolicyName ?? "Unknown Product",
                PolicyType = policy.PolicyType,
                CoverageDetails = product?.CoverageDetails ?? "N/A",
                PlanTier = policy.PlanTier,
                Destination = policy.Destination ?? "N/A",
                TravellerName = policy.TravellerName ?? "Unknown Traveller",
                TravellerAge = policy.TravellerAge,
                PassportNumber = policy.PassportNumber ?? "N/A",
                KycType = policy.KycType ?? "",
                KycNumber = policy.KycNumber ?? "",
                PremiumAmount = policy.PremiumAmount,
                CoverageLimit = product?.CoverageLimit ?? 0,
                ClaimLimit = product?.ClaimLimit ?? 0,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                Status = policy.Status.ToString(),
                CreatedAt = policy.CreatedAt
            };
        }
    }
}