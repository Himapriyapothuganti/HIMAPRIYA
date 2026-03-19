using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;

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
        private readonly INotificationService _notificationService;

        public PolicyRequestService(
            IPolicyRequestRepository requestRepo,
            IPolicyRequestDocumentRepository documentRepo,
            IPolicyProductRepository productRepo,
            IPolicyRepository policyRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _requestRepo = requestRepo;
            _documentRepo = documentRepo;
            _productRepo = productRepo;
            _policyRepo = policyRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _notificationService = notificationService;
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

            // 1.5. Age Eligibility Check
            if (product.PolicyType == "Student" && dto.TravellerAge > 35)
            {
                throw new Exception("Student plans are only available for travellers aged 35 and below.");
            }
            if (product.PolicyType == "Single Trip" && dto.TravellerAge > 99) // Realistic cap
            {
                throw new Exception("Traveller age exceeds the maximum limit for this plan.");
            }


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

            int effectiveAge = dto.TravellerAge;
            int memberCount = 1;

            if (product.PolicyType == "Family" && !string.IsNullOrWhiteSpace(dto.Dependents))
            {
                try
                {
                    var dependents = JsonSerializer.Deserialize<List<DependentDTO>>(dto.Dependents);
                    if (dependents != null && dependents.Any())
                    {
                        var maxDependentAge = dependents.Max(d => d.Age);
                        if (maxDependentAge > effectiveAge)
                        {
                            effectiveAge = maxDependentAge; // Age loading based on OLDEST member
                        }
                        memberCount += dependents.Count; // Traveller + dependents
                    }
                }
                catch { } // Default fallback
            }

            // 6. Calculate Premium & Risk
            var premiumAmount = CalculatePremium(product.BasePremium, days, effectiveAge, product.PolicyType, memberCount, dto.Destination);
            var risk = CalculateRiskScore(effectiveAge, dto.Destination, days, product.PlanTier);

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
                
                Dependents = dto.Dependents,
                UniversityName = dto.UniversityName,
                StudentId = dto.StudentId,
                TripFrequency = dto.TripFrequency,

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

            var customer = await _userManager.FindByIdAsync(customerId);
            string custName = customer?.FullName ?? "a Customer";

            if (!string.IsNullOrEmpty(assignedAgentId))
            {
                var agent = await _userManager.FindByIdAsync(assignedAgentId);
                string agentName = agent?.FullName ?? "an Agent";

                // Notify Customer
                string custMessage = $"Your policy request for {product.PolicyName} has been successfully submitted! It has been assigned to Agent {agentName}.";
                await _notificationService.CreateNotificationAsync(customerId, custMessage, "PolicyRequestSubmitted");

                // Notify Agent
                string agentMessage = $"You have been assigned a new policy request from {custName} for destination {dto.Destination}.";
                await _notificationService.CreateNotificationAsync(assignedAgentId, agentMessage, "PolicyRequestAssigned");
            }
            else
            {
                // Fallback if no agent is somehow assigned
                string custMessage = $"Your policy request for {product.PolicyName} has been successfully submitted!";
                await _notificationService.CreateNotificationAsync(customerId, custMessage, "PolicyRequestSubmitted");
            }

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

        private static decimal GetDestinationMultiplier(string destination)
        {
            var destLower = (destination ?? "").ToLower();
            if (destLower.Contains("asia")) return 1.0m;
            if (destLower.Contains("europe")) return 1.2m;
            if (destLower.Contains("usa") || destLower.Contains("aus") || destLower.Contains("canada")) return 1.5m;
            if (destLower.Contains("worldwide")) return 1.8m;
            return 1.0m; // Default
        }

        private static decimal CalculatePremium(decimal basePremium, int days, int age, string policyType, int memberCount, string destination)
        {
            decimal ageLoading = 1.0m;
            if (age > 60) ageLoading = 1.3m;
            else if (age > 40) ageLoading = 1.1m;

            decimal destMultiplier = GetDestinationMultiplier(destination);

            if (policyType == "Multi-Trip" || policyType == "Student")
            {
                // Fixed annual premium, no days calc, but apply loading/multiplier
                return Math.Round(basePremium * ageLoading * destMultiplier, 2);
            }
            
            decimal daysRatio = days / 30m;

            if (policyType == "Family")
            {
                decimal multiplier = 1.0m;
                if (memberCount == 2) multiplier = 1.5m;
                else if (memberCount == 3) multiplier = 1.8m;
                else if (memberCount == 4) multiplier = 2.0m;
                else if (memberCount == 5) multiplier = 2.2m;
                else if (memberCount >= 6) multiplier = 2.5m;

                return Math.Round(basePremium * daysRatio * ageLoading * multiplier * destMultiplier, 2);
            }
            
            // Single Trip
            return Math.Round(basePremium * daysRatio * ageLoading * destMultiplier, 2);
        }

        private static (int Total, int Age, int Destination, int Duration, int Tier, string Level) CalculateRiskScore(int age, string destination, int days, string planTier)
        {
            int ageScore = age <= 30 ? 0 : (age <= 40 ? 10 : (age <= 55 ? 20 : 30));
            
            int destScore = 0;
            var destLower = (destination ?? "").ToLower();
            if (destLower.Contains("asia")) destScore = 5;
            else if (destLower.Contains("europe")) destScore = 15;
            else if (destLower.Contains("usa") || destLower.Contains("aus") || destLower.Contains("canada")) destScore = 25;
            else if (destLower.Contains("worldwide")) destScore = 40;
            else destScore = 10; // Default unknown

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

            var product = await _productRepo.GetByIdAsync(req.PolicyProductId);
            var agent = await _userManager.FindByIdAsync(agentId);

            if (dto.Status == "Approved")
            {
                string message = $"Your policy request for {product.PolicyName} has been Approved by your assigned Agent, {agent?.FullName}. Please proceed to payment to activate your policy.";
                await _notificationService.CreateNotificationAsync(req.CustomerId, message, "PolicyApproved");
            }
            else if (dto.Status == "Rejected")
            {
                string message = $"Your policy request for {product.PolicyName} was Rejected by your assigned Agent, {agent?.FullName}. Reason: {dto.RejectionReason}.";
                await _notificationService.CreateNotificationAsync(req.CustomerId, message, "PolicyRejected");
            }

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

        public async Task<PolicyRequestResponseDTO> UpdateRequestAsync(int requestId, string customerId, CreatePolicyRequestDTO dto, IFormFile? kycFile, IFormFile? passportFile, IFormFile? otherFile)
        {
            // 1. Fetch and Validate
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (request == null) throw new Exception("Request not found.");
            if (request.CustomerId != customerId) throw new Exception("Unauthorized.");
            if (request.Status != "Rejected") throw new Exception("Only rejected requests can be updated.");

            var product = await _productRepo.GetByIdAsync(dto.PolicyProductId);
            if (product == null) throw new Exception("Product not found.");

            // 2. Validate Dates
            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new Exception("Start date cannot be in the past.");
            if (dto.EndDate <= dto.StartDate)
                throw new Exception("End date must be after start date.");

            var days = (dto.EndDate - dto.StartDate).Days;
            if (days > product.Tenure)
                throw new Exception($"Trip duration exceeds plan limit of {product.Tenure} days.");

            // 3. KYC Validation
            ValidateKyc(dto.KycType, dto.KycNumber);

            // 3.5 Age Eligibility Check
            if (product.PolicyType == "Student" && dto.TravellerAge > 35)
            {
                throw new Exception("Student plans are only available for travellers aged 35 and below.");
            }
            if (product.PolicyType == "Single Trip" && dto.TravellerAge > 99)
            {
                throw new Exception("Traveller age exceeds the maximum limit for this plan.");
            }

            // 4. Update Details
            request.Destination = dto.Destination;
            request.StartDate = dto.StartDate;
            request.EndDate = dto.EndDate;
            request.TravellerName = dto.TravellerName;
            request.TravellerAge = dto.TravellerAge;
            request.PassportNumber = dto.PassportNumber;
            request.KycType = dto.KycType;
            request.KycNumber = dto.KycNumber;
            request.Dependents = dto.Dependents;
            request.UniversityName = dto.UniversityName;
            request.StudentId = dto.StudentId;
            request.TripFrequency = dto.TripFrequency;

            // 5. Recalculate
            int effectiveAge = dto.TravellerAge;
            int memberCount = 1;

            if (product.PolicyType == "Family" && !string.IsNullOrWhiteSpace(dto.Dependents))
            {
                try
                {
                    var dependents = JsonSerializer.Deserialize<List<DependentDTO>>(dto.Dependents);
                    if (dependents != null && dependents.Any())
                    {
                        var maxDependentAge = dependents.Max(d => d.Age);
                        if (maxDependentAge > effectiveAge) effectiveAge = maxDependentAge;
                        memberCount += dependents.Count;
                    }
                }
                catch { }
            }

            request.CalculatedPremium = CalculatePremium(product.BasePremium, days, effectiveAge, product.PolicyType, memberCount, dto.Destination);
            var risk = CalculateRiskScore(effectiveAge, dto.Destination, days, product.PlanTier);
            
            request.RiskScore = risk.Total;
            request.RiskAgeScore = risk.Age;
            request.RiskDestinationScore = risk.Destination;
            request.RiskDurationScore = risk.Duration;
            request.RiskTierScore = risk.Tier;
            request.RiskLevel = risk.Level;

            // 6. Reset Status
            request.Status = "Pending";
            request.RejectionReason = null;
            request.ReviewedAt = null;
            request.RequestedAt = DateTime.UtcNow; // Updated time

            await _requestRepo.UpdateAsync(request);

            // 7. Handle Files
            if (kycFile != null && kycFile.Length > 0)
                await SaveDocumentAsync(requestId, kycFile, "KYC");
            
            if (passportFile != null && passportFile.Length > 0)
                await SaveDocumentAsync(requestId, passportFile, "Passport");
            
            if (otherFile != null && otherFile.Length > 0)
                await SaveDocumentAsync(requestId, otherFile, "Other");
            
            await _documentRepo.SaveChangesAsync();

            // 8. Notify Agent
            if (!string.IsNullOrEmpty(request.AgentId))
            {
                var customer = await _userManager.FindByIdAsync(customerId);
                var agentMessage = $"Rejected request for {request.PolicyProduct?.PolicyName} has been resubmitted by {customer?.FullName ?? "the Customer"}.";
                await _notificationService.CreateNotificationAsync(request.AgentId, agentMessage, "PolicyRequestResubmitted");
            }

            return await MapToCustomerResponse(request);
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
