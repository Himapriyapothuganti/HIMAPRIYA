using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly IPolicyProductRepository _productRepo;
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClaimDocumentRepository _claimDocumentRepo; 

        public ClaimService(
            IClaimRepository claimRepo,
            IPolicyRepository policyRepo,
            IPolicyProductRepository productRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager,
            IClaimDocumentRepository claimDocumentRepo)  
        {
            _claimRepo = claimRepo;
            _policyRepo = policyRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _claimDocumentRepo = claimDocumentRepo;  
        }

        // ── SUBMIT CLAIM (Customer) ───────────────────────
        //BUSINESS LOGIC (The brain of the operation)
        public async Task<ClaimResponseDTO> SubmitClaimAsync(string customerId, CreateClaimDTO dto)
        {
            // Validation Check 1: Does this policy exist and belong to this customer?
            var policy = await _policyRepo.GetByIdAsync(dto.PolicyId);

            if (policy.CustomerId != customerId)
                throw new Exception("You are not authorized to claim on this policy.");

            // Validation Check 2: Is the policy in a claimable state?
            if (policy.Status != PolicyStatus.Active && policy.Status != PolicyStatus.Expired)
                throw new Exception("Only Active or Expired policies are eligible for claims.");

            // Validation Check 3: Is IncidentDate within the policy period?
            if (dto.IncidentDate.Date < policy.StartDate.Date || dto.IncidentDate.Date > policy.EndDate.Date)
                throw new Exception($"Incident date must fall between the policy start date ({policy.StartDate.ToShortDateString()}) and end date ({policy.EndDate.ToShortDateString()}).");

            // Validation Check 3.5: Cannot submit claim for an incident that hasn't happened yet
            if (dto.IncidentDate.Date > DateTime.Now.Date)
                throw new Exception("Incident date cannot be in the future.");

            //  Validation Check 4: Is claimed amount within the product's claim limit?
            var product = await _productRepo.GetByIdAsync(policy.PolicyProductId);
            if (dto.ClaimedAmount > product.ClaimLimit)
                throw new Exception($"Claimed amount exceeds the claim limit of {product.ClaimLimit}.");

            //  Find the Claims Officer with the fewest active claims
            // This ensures work is distributed fairly among all available officers
            var officers = await _userRepo.GetByRoleAsync(UserRole.ClaimsOfficer);
            var activeOfficers = officers.Where(o => o.IsActive).ToList();
            if (!activeOfficers.Any())
                throw new Exception("No active claims officers available to handle this request.");

            // Loop through all officers and pick the one with the least workload
            string assignedOfficerId = activeOfficers.First().Id;
            int minClaims = int.MaxValue;

            foreach (var officer in activeOfficers)
            {
                var count = await _claimRepo.GetActiveClaimCountByOfficerAsync(officer.Id);
                if (count < minClaims)
                {
                    minClaims = count;
                    assignedOfficerId = officer.Id;
                }
            }

            // Build the Claim object and save it to the database
            var claim = new Claim
            {
                PolicyId = dto.PolicyId,
                CustomerId = customerId,
                ClaimType = dto.ClaimType,
                Description = dto.Description,
                ClaimedAmount = dto.ClaimedAmount,
                IncidentDate = dto.IncidentDate,
                Status = ClaimStatus.UnderReview,     // Default status when first submitted
                ClaimsOfficerId = assignedOfficerId,  // Auto-assigned officer
                SubmittedAt = DateTime.UtcNow
            };

            // → This calls ClaimRepository.CreateAsync() — fires the SQL INSERT
            var created = await _claimRepo.CreateAsync(claim);

            // ── Save Uploaded Documents ───────────────────
            if (dto.Documents != null && dto.Documents.Any())
            {
                var uploadFolder = Path.Combine("wwwroot", "claims", created.ClaimId.ToString());
                Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.Documents)
                {
                    if (file.Length > 0)
                    {
                        // Validate file type to prevent malicious uploads (like .exe or scripts)
                        var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedTypes.Contains(extension))
                            throw new Exception($"File {file.FileName} not allowed. Only PDF, JPG, PNG accepted.");

                        // Validate file size (max 5MB)
                        if (file.Length > 5 * 1024 * 1024)
                            throw new Exception($"File {file.FileName} exceeds 5MB limit.");

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        await _claimDocumentRepo.AddAsync(new ClaimDocument
                        {
                            ClaimId = created.ClaimId,
                            FileName = file.FileName,
                            FilePath = filePath,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                await _claimDocumentRepo.SaveChangesAsync();
            }

            return await MapToClaimResponse(created);
        }

        // ── GET MY CLAIMS (Customer) ──────────────────────
        public async Task<List<ClaimResponseDTO>> GetMyClaimsAsync(string customerId)
        {
            var claims = await _claimRepo.GetByCustomerIdAsync(customerId);
            var result = new List<ClaimResponseDTO>();
            foreach (var c in claims)
                result.Add(await MapToClaimResponse(c));
            return result;
        }

        // ── GET CLAIM BY ID (Customer) ────────────────────
        public async Task<ClaimResponseDTO> GetClaimByIdAsync(int claimId, string customerId)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim.CustomerId != customerId)
                throw new Exception("You are not authorized to view this claim.");
            return await MapToClaimResponse(claim);
        }

        // ── GET ASSIGNED CLAIMS (Claims Officer) ──────────
        public async Task<List<ClaimResponseDTO>> GetAssignedClaimsAsync(string officerId)
        {
            var claims = await _claimRepo.GetByOfficerIdAsync(officerId);
            var result = new List<ClaimResponseDTO>();
            foreach (var c in claims)
                result.Add(await MapToClaimResponse(c));
            return result;
        }

        // ── REVIEW CLAIM (Claims Officer) ─────────────────
        public async Task<ClaimResponseDTO> ReviewClaimAsync(int claimId, string officerId, ReviewClaimDTO dto)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);

            if (claim.ClaimsOfficerId != officerId)
                throw new Exception("You are not assigned to this claim.");

            if (claim.Status != ClaimStatus.UnderReview && claim.Status != ClaimStatus.PendingDocuments)
                throw new Exception("Claim is not in a reviewable state.");

            if (dto.IsApproved)
            {
                if (dto.ApprovedAmount == null || dto.ApprovedAmount <= 0)
                    throw new Exception("Approved amount is required when approving a claim.");

                // Note: The ApprovedAmount might be less than ClaimedAmount if the officer 
                // determines the claim exceeds policy coverage limits or lacks sufficient evidence.
                claim.ApprovedAmount = dto.ApprovedAmount;
                claim.Status = ClaimStatus.Approved;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                    throw new Exception("Rejection reason is required when rejecting a claim.");

                claim.RejectionReason = dto.RejectionReason;
                claim.Status = ClaimStatus.Rejected;
            }

            claim.ReviewedAt = DateTime.UtcNow;
            var updated = await _claimRepo.UpdateAsync(claim);
            return await MapToClaimResponse(updated);
        }

        // ── REQUEST DOCUMENTS (Claims Officer) ────────────
        public async Task<ClaimResponseDTO> RequestDocumentsAsync(int claimId, string officerId, RequestDocumentDTO dto)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);

            if (claim.ClaimsOfficerId != officerId)
                throw new Exception("You are not assigned to this claim.");

            if (claim.Status != ClaimStatus.UnderReview)
                throw new Exception("Can only request documents for claims under review.");

            claim.RejectionReason = dto.Reason;
            claim.Status = ClaimStatus.PendingDocuments;

            var updated = await _claimRepo.UpdateAsync(claim);
            return await MapToClaimResponse(updated);
        }

        // ── PROCESS PAYMENT (Claims Officer) ──────────────
        public async Task<ClaimResponseDTO> ProcessPaymentAsync(int claimId, string officerId)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);

            if (claim.ClaimsOfficerId != officerId)
                throw new Exception("You are not assigned to this claim.");

            // Strict state machine constraint: Payments can only be processed if the claim was formally approved.
            if (claim.Status != ClaimStatus.Approved)
                throw new Exception("Only approved claims can be processed for payment.");

            claim.Status = ClaimStatus.PaymentProcessed;
            var updated = await _claimRepo.UpdateAsync(claim);
            return await MapToClaimResponse(updated);
        }

        // ── CLOSE CLAIM (Claims Officer) ──────────────────
        public async Task<ClaimResponseDTO> CloseClaimAsync(int claimId, string officerId)
        {
            var claim = await _claimRepo.GetByIdAsync(claimId);

            if (claim.ClaimsOfficerId != officerId)
                throw new Exception("You are not assigned to this claim.");

            if (claim.Status != ClaimStatus.PaymentProcessed && claim.Status != ClaimStatus.Rejected)
                throw new Exception("Only PaymentProcessed or Rejected claims can be closed.");

            claim.Status = ClaimStatus.Closed;
            var updated = await _claimRepo.UpdateAsync(claim);
            return await MapToClaimResponse(updated);
        }

        // ── GET ALL CLAIMS (Admin) ─────────────────────────
        public async Task<List<ClaimResponseDTO>> GetAllClaimsAsync()
        {
            var claims = await _claimRepo.GetAllAsync();
            var result = new List<ClaimResponseDTO>();
            foreach (var c in claims)
                result.Add(await MapToClaimResponse(c));
            return result;
        }

        // ── DOWNLOAD DOCUMENT (Shared) ────────────────────
        public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadDocumentAsync(int documentId)
        {
            var document = await _claimDocumentRepo.GetByIdAsync(documentId);
            if (document == null)
                throw new Exception("Document not found.");

            if (!System.IO.File.Exists(document.FilePath))
                throw new Exception("File not found on the server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            return (fileBytes, document.FileType, document.FileName);
        }

        // ── MAPPER ────────────────────────────────────────
        private async Task<ClaimResponseDTO> MapToClaimResponse(Claim claim)
        {
            var customer = await _userManager.FindByIdAsync(claim.CustomerId);
            var officer = claim.ClaimsOfficerId != null
                           ? await _userManager.FindByIdAsync(claim.ClaimsOfficerId)
                           : null;
            var policy = await _policyRepo.GetByIdAsync(claim.PolicyId);

            // ── Get documents ─────────────────────────────
            var docs = await _claimDocumentRepo.GetByClaimIdAsync(claim.ClaimId);
            var documents = docs.Select(d => new ClaimDocumentDTO
            {
                ClaimDocumentId = d.ClaimDocumentId,
                FileName = d.FileName,
                FileType = d.FileType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                FileUrl = $"https://localhost:7161/api/Claim/document/{d.ClaimDocumentId}"
            }).ToList();

            return new ClaimResponseDTO
            {
                ClaimId = claim.ClaimId,
                PolicyId = claim.PolicyId,
                PolicyNumber = policy?.PolicyNumber ?? "Unknown",
                CustomerId = claim.CustomerId,
                CustomerName = customer?.FullName ?? "Unknown Customer",
                ClaimsOfficerId = claim.ClaimsOfficerId,
                ClaimsOfficerName = officer?.FullName ?? "",
                ClaimType = claim.ClaimType,
                Description = claim.Description,
                ClaimedAmount = claim.ClaimedAmount,
                ApprovedAmount = claim.ApprovedAmount,
                Status = claim.Status.ToString(),
                RejectionReason = claim.RejectionReason,
                SubmittedAt = claim.SubmittedAt,
                ReviewedAt = claim.ReviewedAt,
                IncidentDate = claim.IncidentDate,
                Documents = documents
            };
        }
    }
}