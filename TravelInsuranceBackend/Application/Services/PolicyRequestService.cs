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
        private readonly ICountryRiskRepository _countryRiskRepo;
        private readonly IVertexAiService _vertexAiService;

        public PolicyRequestService(
            IPolicyRequestRepository requestRepo,
            IPolicyRequestDocumentRepository documentRepo,
            IPolicyProductRepository productRepo,
            IPolicyRepository policyRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ICountryRiskRepository countryRiskRepo,
            IVertexAiService vertexAiService)
        {
            _requestRepo = requestRepo;
            _documentRepo = documentRepo;
            _productRepo = productRepo;
            _policyRepo = policyRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _notificationService = notificationService;
            _countryRiskRepo = countryRiskRepo;
            _vertexAiService = vertexAiService;
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

            // 6. Validate Destination & Get Multiplier
            var countryRisk = await _countryRiskRepo.GetByNameAsync(dto.Destination);
            if (countryRisk == null || !countryRisk.IsActive)
                throw new Exception("We currently do not provide services for this destination.");

            decimal destMultiplier = countryRisk.Multiplier;

            // 7. Calculate Premium
            var premiumAmount = CalculatePremium(product.BasePremium, product.Tenure, days, effectiveAge, product.PolicyType, memberCount, destMultiplier);

            // 8. Create Request Instance
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
                DestinationRiskMultiplier = destMultiplier 
            };
            
            await _requestRepo.CreateAsync(request); // Save to get the ID for documents

            // 9. Process Documents & Collect Paths for AI
            var filePaths = new List<string>();
            var docsToSave = new List<(IFormFile file, string type)>();
            if (kycFile != null) docsToSave.Add((kycFile, "KYC"));
            if (passportFile != null) docsToSave.Add((passportFile, "Passport"));
            if (otherFile != null) docsToSave.Add((otherFile, "Other"));

            foreach (var doc in docsToSave)
            {
                var uniqueName = $"{Guid.NewGuid()}_{doc.file.FileName}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "policy-docs");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                
                var filePath = Path.Combine(folderPath, uniqueName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doc.file.CopyToAsync(stream);
                }
                filePaths.Add(filePath);

                var dbDoc = new PolicyRequestDocument
                {
                    PolicyRequestId = request.PolicyRequestId,
                    FileName = doc.file.FileName,
                    FilePath = $"/uploads/policy-docs/{uniqueName}",
                    FileType = doc.file.ContentType,
                    DocumentType = doc.type,
                    FileSize = doc.file.Length,
                    UploadedAt = DateTime.UtcNow
                };
                await _documentRepo.AddAsync(dbDoc);
            }
            await _documentRepo.SaveChangesAsync();

            // 10. AI ANALYSIS (Vertex AI)
            var aiRequestJson = JsonSerializer.Serialize(new {
                request.TravellerName,
                request.TravellerAge,
                request.Destination,
                DestinationRiskMultiplier = destMultiplier,
                request.StartDate,
                request.EndDate,
                DurationDays = days,
                product.PolicyName,
                product.PlanTier
            });

            var aiResultJson = await _vertexAiService.AnalyzePolicyRequestAsync(aiRequestJson, filePaths);
            request.AiAnalysisJson = aiResultJson; // Save full report for the unified standard

            try {
                using var aiDoc = JsonDocument.Parse(aiResultJson);
                var root = aiDoc.RootElement;
                
                // Keep structural fields for basic UI components
                if (root.TryGetProperty("riskScore", out var sProp))
                    request.RiskScore = sProp.ValueKind == JsonValueKind.Number ? sProp.GetInt32() : int.Parse(sProp.GetString() ?? "0");
                else if (root.TryGetProperty("RiskScore", out var SProp))
                    request.RiskScore = SProp.ValueKind == JsonValueKind.Number ? SProp.GetInt32() : int.Parse(SProp.GetString() ?? "0");

                if (root.TryGetProperty("riskLevel", out var lProp))
                    request.RiskLevel = lProp.GetString() ?? "Medium";
                else if (root.TryGetProperty("RiskLevel", out var LProp))
                    request.RiskLevel = LProp.GetString() ?? "Medium";

                if (root.TryGetProperty("riskReasoning", out var rProp))
                    request.RiskReasoning = rProp.GetString();
                else if (root.TryGetProperty("RiskReasoning", out var RProp))
                    request.RiskReasoning = RProp.GetString();
                
            }
            catch (Exception ex) {
                Console.WriteLine($"Policy AI Analysis Parsing Error: {ex.Message}");
            }

            await _requestRepo.UpdateAsync(request);
            
            // 11. Notification
            if (!string.IsNullOrEmpty(assignedAgentId))
            {
                var customer = await _userManager.FindByIdAsync(customerId);
                string custName = customer?.FullName ?? "a Customer";
                var agent = await _userManager.FindByIdAsync(assignedAgentId);
                string agentName = agent?.FullName ?? "an Agent";
                await _notificationService.CreateNotificationAsync(customerId, $"Your policy request for {product.PolicyName} has been submitted and assigned to Agent {agentName}.", "PolicyRequestSubmitted");
                await _notificationService.CreateNotificationAsync(assignedAgentId, $"New policy request from {custName} for {dto.Destination}.", "PolicyRequestAssigned");
            }
            else
            {
                await _notificationService.CreateNotificationAsync(customerId, $"Your policy request for {product.PolicyName} has been submitted.", "PolicyRequestSubmitted");
            }

            return await MapToCustomerResponse(request);
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

        private static decimal CalculatePremium(decimal basePremium, int tenure, int days, int age, string policyType, int memberCount, decimal destMultiplier)
        {
            decimal ageLoading = 1.0m;
            if (age > 60) ageLoading = 1.3m;
            else if (age > 40) ageLoading = 1.1m;

            if (policyType == "Multi-Trip" || policyType == "Student")
            {
                // Fixed annual premium, no days calc, but apply loading/multiplier
                return Math.Round(basePremium * ageLoading * destMultiplier, 2);
            }
            
            // Pro-rate based on plan's tenure (e.g. 30 for Single Trip, 365 for Student)
            decimal daysRatio = days / (decimal)tenure;

            decimal calculated;
            if (policyType == "Family")
            {
                decimal multiplier = 1.0m;
                if (memberCount == 2) multiplier = 1.5m;
                else if (memberCount == 3) multiplier = 1.8m;
                else if (memberCount == 4) multiplier = 2.0m;
                else if (memberCount == 5) multiplier = 2.2m;
                else if (memberCount >= 6) multiplier = 2.5m;

                calculated = basePremium * daysRatio * ageLoading * multiplier * destMultiplier;
            }
            else 
            {
                // Single Trip
                calculated = basePremium * daysRatio * ageLoading * destMultiplier;
            }

            // Ensure we never charge less than the base premium
            return Math.Round(Math.Max(calculated, basePremium), 2);
        }

        private string GetRiskLevelFromMultiplier(decimal multiplier)
        {
            if (multiplier <= 1.2m) return "Low";
            if (multiplier <= 1.8m) return "Medium";
            return "High";
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
            else if (dto.Status == "Needs Revision")
            {
                if (string.IsNullOrWhiteSpace(dto.RequestedDocTypes)) throw new Exception("Please specify which documents need revision.");
                req.Status = "Needs Revision";
                req.RequestedDocTypes = dto.RequestedDocTypes;
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
            else if (dto.Status == "Needs Revision")
            {
                string message = $"Action Required: Your Agent, {agent?.FullName}, has requested document revisions for your {product.PolicyName} request. Flagged: {dto.RequestedDocTypes}.";
                await _notificationService.CreateNotificationAsync(req.CustomerId, message, "PolicyRevisionRequired");
            }

            return await MapToAgentResponse(updated);
        }

        public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadDocumentAsync(int documentId, string userId, string userRole)
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null) throw new Exception("Document not found.");

            // IDOR Protection: 
            // 1. Admins, Officers and Agents can see all (for review/audit)
            // 2. Customers can ONLY see their own docs
            if (userRole == "Customer")
            {
                var request = await _requestRepo.GetByIdAsync(document.PolicyRequestId);
                if (request == null || request.CustomerId != userId)
                    throw new Exception("Unauthorized access to this document.");
            }

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
            
            // Allow update if status is Rejected OR Needs Revision
            if (request.Status != "Rejected" && request.Status != "Needs Revision") 
                throw new Exception("Only Rejected or Requests needing Revision can be updated.");

            // Check Resubmission Limit
            if (request.Status == "Needs Revision")
            {
                if (request.ResubmissionCount >= request.MaxResubmissions)
                {
                    request.Status = "Final Rejected"; // Strike out
                    await _requestRepo.UpdateAsync(request);
                    throw new Exception("Maximum resubmission attempts reached. Your request has been final rejected.");
                }
                request.ResubmissionCount++;
            }

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

            // 5. Validate Destination & Get Multiplier
            var countryRisk = await _countryRiskRepo.GetByNameAsync(dto.Destination);
            if (countryRisk == null || !countryRisk.IsActive)
                throw new Exception("We currently do not provide services for this destination.");

            decimal destMultiplier = countryRisk.Multiplier;

            // 6. Recalculate Premium
            request.CalculatedPremium = CalculatePremium(product.BasePremium, product.Tenure, days, effectiveAge, product.PolicyType, memberCount, destMultiplier);
            request.DestinationRiskMultiplier = destMultiplier;

            // 6. Handle Status Reset
            request.Status = "Pending";
            request.RejectionReason = null;
            
            // → Correct migration/update call
            await _requestRepo.UpdateAsync(request);

            // 7. Handle File Replacement Logic (IsLatest)
            if (kycFile != null) await MarkOldDocsNotLatest(requestId, "KYC");
            if (passportFile != null) await MarkOldDocsNotLatest(requestId, "Passport");
            if (otherFile != null) await MarkOldDocsNotLatest(requestId, "Other");
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

            // 8. AI RE-ANALYSIS
            var allDocs = await _documentRepo.GetByRequestIdAsync(requestId);
            var latestFilePaths = allDocs.Where(d => d.IsLatest).Select(d => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", d.FilePath.TrimStart('/'))).ToList();
            
            var aiRequestJson = JsonSerializer.Serialize(new {
                request.TravellerName,
                request.TravellerAge,
                request.Destination,
                DestinationRiskMultiplier = destMultiplier,
                request.StartDate,
                request.EndDate,
                DurationDays = (request.EndDate - request.StartDate).Days,
                product.PolicyName,
                product.PlanTier
            });

            var aiResultJson = await _vertexAiService.AnalyzePolicyRequestAsync(aiRequestJson, latestFilePaths);
            try {
                using var aiDoc = JsonDocument.Parse(aiResultJson);
                var root = aiDoc.RootElement;
                request.RiskScore = root.GetProperty("riskScore").GetInt32();
                request.RiskLevel = root.GetProperty("riskLevel").GetString() ?? "Medium";
                request.RiskReasoning = root.GetProperty("riskReasoning").GetString();
            }
            catch {
                request.RiskReasoning = "AI re-analysis pending new document review.";
            }

            await _requestRepo.UpdateAsync(request);

            // 9. Notify Agent
            if (!string.IsNullOrEmpty(request.AgentId))
            {
                var customer = await _userManager.FindByIdAsync(customerId);
                var agentMessage = $"Rejected request for {product.PolicyName} has been resubmitted by {customer?.FullName ?? "the Customer"}.";
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
            var allDocs = await _documentRepo.GetByRequestIdAsync(req.PolicyRequestId);
            
            // IMPORTANT: Only show "IsLatest" documents to prevent confusion
            var documentDtos = allDocs
                .Where(d => d.IsLatest) 
                .Select(d => new PolicyRequestDocumentDTO
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
                ResubmissionCount = req.ResubmissionCount,
                MaxResubmissions = req.MaxResubmissions,
                RequestedDocTypes = req.RequestedDocTypes,
                RequestedAt = req.RequestedAt,
                ReviewedAt = req.ReviewedAt,
                
                CustomerName = req.Customer?.FullName ?? "Unknown",
                AgentNotes = req.AgentNotes,
                
                RiskScore = req.RiskScore,
                RiskLevel = req.RiskLevel,
                RiskReasoning = req.RiskReasoning,
                CountryRiskMultiplier = req.DestinationRiskMultiplier,
                CountryRiskLevel = GetRiskLevelFromMultiplier(req.DestinationRiskMultiplier),
                AiAnalysisJson = req.AiAnalysisJson,
                
                Documents = documentDtos
            };
        }

        private async Task MarkOldDocsNotLatest(int requestId, string docType)
        {
            var docs = await _documentRepo.GetByRequestIdAsync(requestId);
            var targets = docs.Where(d => d.DocumentType == docType && d.IsLatest).ToList();
            foreach (var doc in targets)
            {
                doc.IsLatest = false;
            }
            await _documentRepo.SaveChangesAsync();
        }

        public async Task<PremiumCalculationResponseDTO> CalculatePremiumPreviewAsync(PremiumCalculationRequestDTO dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.PolicyProductId);
            if (product == null) throw new Exception("Product not found.");

            var countryRisk = await _countryRiskRepo.GetByNameAsync(dto.Destination);
            if (countryRisk == null || !countryRisk.IsActive)
                throw new Exception("This destination is not supported by Talk & Travel.");

            decimal destMultiplier = countryRisk.Multiplier;
            decimal ageLoading = dto.TravellerAge > 60 ? 1.3m : (dto.TravellerAge > 40 ? 1.1m : 1.0m);

            var days = (dto.EndDate - dto.StartDate).Days;
            var premium = CalculatePremium(product.BasePremium, product.Tenure, days, dto.TravellerAge, product.PolicyType, dto.MemberCount, destMultiplier);
            var estimateLevel = GetRiskLevelFromMultiplier(destMultiplier);

            return new PremiumCalculationResponseDTO
            {
                EstimatedPremium = premium,
                DestinationMultiplier = destMultiplier,
                AgeLoading = ageLoading,
                RiskLevel = estimateLevel
            };
        }
    }
}
