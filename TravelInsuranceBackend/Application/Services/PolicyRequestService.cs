using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PolicyRequestService : IPolicyRequestService
    {
        private readonly IPolicyRequestRepository _requestRepo;
        private readonly IPolicyRequestDocumentRepository _documentRepo;
        private readonly IPolicyProductRepository _productRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public PolicyRequestService(
            IPolicyRequestRepository requestRepo,
            IPolicyRequestDocumentRepository documentRepo,
            IPolicyProductRepository productRepo,
            IPolicyRepository policyRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager)
        {
            _requestRepo = requestRepo;
            _documentRepo = documentRepo;
            _productRepo = productRepo;
            _policyRepo = policyRepo;
            _userRepo = userRepo;
            _userManager = userManager;
        }

        public async Task<PolicyRequestResponseDTO> SubmitRequestAsync(string customerId, CreatePolicyRequestDTO dto, IFormFile kycFile, IFormFile passportFile, IFormFile? otherFile)
        {
            // 1. Validate Product
            var product = await _productRepo.GetByIdAsync(dto.PolicyProductId);
            if (product.Status != PolicyProductStatus.Available)
                throw new Exception("This policy product is not available.");

            // Check: customer must not already have an Active policy for the same product
            var existingActivePolicies = await _policyRepo.GetByCustomerIdAsync(customerId);
            bool hasDuplicatePolicy = existingActivePolicies.Any(p =>
                p.PolicyProductId == dto.PolicyProductId &&
                p.Status == PolicyStatus.Active);
            if (hasDuplicatePolicy)
                throw new Exception("You already have an active policy for this plan.");

            // Check: customer must not already have a Pending or Approved request for the same product
            var existingRequests = await _requestRepo.GetByCustomerIdAsync(customerId);
            bool hasDuplicateRequest = existingRequests.Any(r =>
                r.PolicyProductId == dto.PolicyProductId &&
                (r.Status == "Pending" || r.Status == "Approved"));
            if (hasDuplicateRequest)
                throw new Exception("You already have a pending or approved request for this plan.");


            // 2. Validate Dates
            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new Exception("Start date cannot be in the past.");
            if (dto.EndDate <= dto.StartDate)
                throw new Exception("End date must be after start date.");

            var days = (dto.EndDate - dto.StartDate).Days;
            if (days > product.Tenure)
                throw new Exception($"Trip duration exceeds plan limit of {product.Tenure} days.");

            // 3. KYC Validation using Regex
            ValidateKyc(dto.KycType, dto.KycNumber);

            // 4. File Validation
            if (kycFile == null || kycFile.Length == 0) throw new Exception("KYC Document is required.");
            if (passportFile == null || passportFile.Length == 0) throw new Exception("Passport Document is required.");

            // 5. Assign Agent
            var existingPolicies = await _policyRepo.GetByCustomerIdAsync(customerId);
            var latestPolicy = existingPolicies.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            string? assignedAgentId = latestPolicy?.AgentId;

            if (string.IsNullOrEmpty(assignedAgentId))
            {
                // Fallback: assign to the agent with the least active requests
                var agents = await _userRepo.GetByRoleAsync(UserRole.Agent);
                var activeAgents = agents.Where(a => a.IsActive).ToList();
                if (activeAgents.Any())
                {
                    int minRequests = int.MaxValue;
                    foreach (var agent in activeAgents)
                    {
                        var count = await _requestRepo.GetActiveRequestCountByAgentAsync(agent.Id);
                        if (count < minRequests)
                        {
                            minRequests = count;
                            assignedAgentId = agent.Id;
                        }
                    }
                }
            }

            // 6. Calculate Premium & Risk
            var premiumAmount = CalculatePremium(product.BasePremium, days, dto.TravellerAge);
            var risk = CalculateRiskScore(dto.TravellerAge, dto.Destination, days, product.PlanTier);

            // 7. Create Request
            var request = new PolicyRequest
            {
                PolicyProductId = dto.PolicyProductId,
                CustomerId = customerId,
                AgentId = assignedAgentId,
                Destination = dto.Destination,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TravellerName = dto.TravellerName,
                TravellerAge = dto.TravellerAge,
                PassportNumber = dto.PassportNumber,
                KycType = dto.KycType,
                KycNumber = dto.KycNumber,
                CalculatedPremium = premiumAmount,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                
                RiskScore = risk.Total,
                RiskAgeScore = risk.Age,
                RiskDestinationScore = risk.Destination,
                RiskDurationScore = risk.Duration,
                RiskTierScore = risk.Tier,
                RiskLevel = risk.Level
            };

            var created = await _requestRepo.CreateAsync(request);

            // 8. Save Documents
            await SaveDocumentAsync(created.PolicyRequestId, kycFile, "KYC");
            await SaveDocumentAsync(created.PolicyRequestId, passportFile, "Passport");
            if (otherFile != null && otherFile.Length > 0)
            {
                await SaveDocumentAsync(created.PolicyRequestId, otherFile, "Other");
            }
            await _documentRepo.SaveChangesAsync();

            return await MapToCustomerResponse(created);
        }

        private async Task SaveDocumentAsync(int requestId, IFormFile file, string documentType)
        {
            var uploadFolder = Path.Combine("wwwroot", "policy-requests", requestId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedTypes.Contains(extension))
                throw new Exception($"File {file.FileName} not allowed. Only PDF, JPG, PNG accepted.");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception($"File {file.FileName} exceeds 5MB limit.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            await _documentRepo.AddAsync(new PolicyRequestDocument
            {
                PolicyRequestId = requestId,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = file.ContentType,
                DocumentType = documentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            });
        }

        private void ValidateKyc(string type, string number)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(number))
                throw new Exception("KYC Type and Number are required.");

            if (type.StartsWith("Aadhaar", StringComparison.OrdinalIgnoreCase))
            {
                if (!Regex.IsMatch(number, @"^\d{12}$"))
                    throw new Exception("Invalid Aadhaar format. Must be 12 digits.");
            }
            else if (type.StartsWith("PAN", StringComparison.OrdinalIgnoreCase))
            {
                if (!Regex.IsMatch(number, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                    throw new Exception("Invalid PAN format.");
            }
            else
            {
                if (number.Length < 5)
                    throw new Exception("Invalid KYC number format.");
            }
        }

        private static decimal CalculatePremium(decimal basePremium, int days, int age)
        {
            var premium = basePremium * (days / 30m);
            if (age > 60) premium *= 1.3m;
            else if (age > 40) premium *= 1.1m;
            return Math.Round(premium, 2);
        }

        private static (int Total, int Age, int Destination, int Duration, int Tier, string Level) CalculateRiskScore(int age, string destination, int days, string planTier)
        {
            int ageScore = age <= 30 ? 0 : (age <= 40 ? 10 : (age <= 55 ? 20 : 30));
            
            int destScore = 0;
            var destLower = destination.ToLower();
            if (destLower.Contains("asia")) destScore = 5;
            else if (destLower.Contains("europe")) destScore = 15;
            else if (destLower.Contains("usa") || destLower.Contains("aus")) destScore = 20;
            else if (destLower.Contains("worldwide")) destScore = 30;

            int durationScore = days <= 7 ? 0 : (days <= 15 ? 5 : (days <= 30 ? 10 : 20));

            int tierScore = 0;
            var tierLower = planTier.ToLower();
            if (tierLower.Contains("silver")) tierScore = 20;
            else if (tierLower.Contains("gold")) tierScore = 10;
            else if (tierLower.Contains("platinum")) tierScore = 0;

            int total = ageScore + destScore + durationScore + tierScore;
            total = Math.Min(100, Math.Max(0, total));

            string level = total <= 30 ? "Low" : (total <= 60 ? "Medium" : "High");

            return (total, ageScore, destScore, durationScore, tierScore, level);
        }

        public async Task<List<PolicyRequestResponseDTO>> GetMyRequestsAsync(string customerId)
        {
            var requests = await _requestRepo.GetByCustomerIdAsync(customerId);
            var result = new List<PolicyRequestResponseDTO>();
            foreach (var req in requests)
            {
                result.Add(await MapToCustomerResponse(req));
            }
            return result;
        }

        public async Task<List<AgentPolicyRequestResponseDTO>> GetAgentRequestsAsync(string agentId)
        {
            var requests = await _requestRepo.GetByAgentIdAsync(agentId);
            var result = new List<AgentPolicyRequestResponseDTO>();
            foreach (var req in requests)
            {
                result.Add(await MapToAgentResponse(req));
            }
            return result;
        }

        public async Task<AgentPolicyRequestResponseDTO> GetRequestByIdAsync(int requestId, string userId, bool isAgent)
        {
            var req = await _requestRepo.GetByIdAsync(requestId);
            if (isAgent && req.AgentId != userId) throw new Exception("Not authorized to view this request.");
            if (!isAgent && req.CustomerId != userId) throw new Exception("Not authorized to view this request.");
            
            return await MapToAgentResponse(req); // Return full response, controller will filter for customer
        }

        public async Task<AgentPolicyRequestResponseDTO> ReviewRequestAsync(int requestId, string agentId, ReviewPolicyRequestDTO dto)
        {
            var req = await _requestRepo.GetByIdAsync(requestId);
            if (req.AgentId != agentId) throw new Exception("Not assigned to this request.");
            if (req.Status != "Pending") throw new Exception("Request is not pending.");

            if (dto.Status == "Approved")
            {
                req.Status = "Approved";
                req.AgentNotes = dto.AgentNotes;
            }
            else if (dto.Status == "Rejected")
            {
                if (string.IsNullOrWhiteSpace(dto.RejectionReason)) throw new Exception("Rejection reason required.");
                req.Status = "Rejected";
                req.RejectionReason = dto.RejectionReason;
                req.AgentNotes = dto.AgentNotes;
            }
            else
            {
                throw new Exception("Invalid review status.");
            }

            req.ReviewedAt = DateTime.UtcNow;
            var updated = await _requestRepo.UpdateAsync(req);
            return await MapToAgentResponse(updated);
        }

        public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadDocumentAsync(int documentId)
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null) throw new Exception("Document not found.");
            if (!System.IO.File.Exists(document.FilePath)) throw new Exception("File not found on the server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            return (fileBytes, document.FileType, document.FileName);
        }

        private async Task<PolicyRequestResponseDTO> MapToCustomerResponse(PolicyRequest req)
        {
            return new PolicyRequestResponseDTO
            {
                PolicyRequestId = req.PolicyRequestId,
                PolicyProductId = req.PolicyProductId,
                PolicyName = req.PolicyProduct?.PolicyName ?? "Unknown",
                PolicyType = req.PolicyProduct?.PolicyType ?? "Unknown",
                PlanTier = req.PolicyProduct?.PlanTier ?? "Unknown",
                Destination = req.Destination,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                TravellerName = req.TravellerName,
                TravellerAge = req.TravellerAge,
                PassportNumber = req.PassportNumber,
                KycType = req.KycType,
                KycNumber = req.KycNumber,
                Status = req.Status,
                RejectionReason = req.RejectionReason,
                CalculatedPremium = req.CalculatedPremium,
                RequestedAt = req.RequestedAt,
                ReviewedAt = req.ReviewedAt
            };
        }

        private async Task<AgentPolicyRequestResponseDTO> MapToAgentResponse(PolicyRequest req)
        {
            var docs = await _documentRepo.GetByRequestIdAsync(req.PolicyRequestId);
            var documentDtos = docs.Select(d => new PolicyRequestDocumentDTO
            {
                PolicyRequestDocumentId = d.PolicyRequestDocumentId,
                FileName = d.FileName,
                FileType = d.FileType,
                DocumentType = d.DocumentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                FileUrl = $"https://localhost:7161/api/PolicyRequest/document/{d.PolicyRequestDocumentId}"
            }).ToList();

            return new AgentPolicyRequestResponseDTO
            {
                PolicyRequestId = req.PolicyRequestId,
                PolicyProductId = req.PolicyProductId,
                PolicyName = req.PolicyProduct?.PolicyName ?? "Unknown",
                PolicyType = req.PolicyProduct?.PolicyType ?? "Unknown",
                PlanTier = req.PolicyProduct?.PlanTier ?? "Unknown",
                Destination = req.Destination,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                TravellerName = req.TravellerName,
                TravellerAge = req.TravellerAge,
                PassportNumber = req.PassportNumber,
                KycType = req.KycType,
                KycNumber = req.KycNumber,
                Status = req.Status,
                RejectionReason = req.RejectionReason,
                CalculatedPremium = req.CalculatedPremium,
                RequestedAt = req.RequestedAt,
                ReviewedAt = req.ReviewedAt,
                
                CustomerName = req.Customer?.FullName ?? "Unknown",
                AgentNotes = req.AgentNotes,
                
                RiskScore = req.RiskScore,
                RiskAgeScore = req.RiskAgeScore,
                RiskDestinationScore = req.RiskDestinationScore,
                RiskDurationScore = req.RiskDurationScore,
                RiskTierScore = req.RiskTierScore,
                RiskLevel = req.RiskLevel,
                
                Documents = documentDtos
            };
        }
    }
}
