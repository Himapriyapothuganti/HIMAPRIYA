using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimRepository>         _claimRepoMock        = new();
        private readonly Mock<IPolicyRepository>         _policyRepoMock       = new();
        private readonly Mock<IPolicyProductRepository>  _productRepoMock      = new();
        private readonly Mock<IUserRepository>           _userRepoMock         = new();
        private readonly Mock<IClaimDocumentRepository>  _claimDocRepoMock     = new();
        private readonly Mock<INotificationService>      _notificationServiceMock = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public ClaimServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private ClaimService CreateService() =>
            new(_claimRepoMock.Object, _policyRepoMock.Object,
                _productRepoMock.Object, _userRepoMock.Object,
                _userManagerMock.Object, _claimDocRepoMock.Object,
                _notificationServiceMock.Object);

        private Claim BuildClaim(string officerId, ClaimStatus status) => new()
        {
            ClaimId         = 1,
            PolicyId        = 10,
            CustomerId      = "cust-001",
            ClaimsOfficerId = officerId,
            ClaimType       = "Medical",
            Description     = "Hospital",
            ClaimedAmount   = 5000m,
            Status          = status
        };

        [Fact]
        public async Task ReviewClaim_ApprovesSuccessfully_WhenValidAmountProvided()
        {
            // Arrange
            var claim = BuildClaim("officer-001", ClaimStatus.UnderReview);
            _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Claim>()))
                          .ReturnsAsync((Domain.Entities.Claim c) => c);
            _policyRepoMock.Setup(r => r.GetByIdAsync(10))
                           .ReturnsAsync(new Policy { PolicyId = 10, PolicyNumber = "POL-001" });
            _claimDocRepoMock.Setup(r => r.GetByClaimIdAsync(1))
                             .ReturnsAsync(new List<ClaimDocument>());
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((ApplicationUser?)null);

            var dto     = new ReviewClaimDTO { IsApproved = true, ApprovedAmount = 4500m };
            var service = CreateService();

            // Act
            var result = await service.ReviewClaimAsync(1, "officer-001", dto);

            // Assert
            Assert.Equal("Approved", result.Status);
            Assert.Equal(4500m,      result.ApprovedAmount);
        }

        [Fact]
        public async Task ReviewClaim_ThrowsException_WhenOfficerNotAssigned()
        {
            // Arrange
            var claim = BuildClaim("officer-001", ClaimStatus.UnderReview);
            _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);

            var dto     = new ReviewClaimDTO { IsApproved = true, ApprovedAmount = 4500m };
            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ReviewClaimAsync(1, "officer-999", dto));
            Assert.Equal("You are not assigned to this claim.", ex.Message);
        }

        [Fact]
        public async Task ReviewClaim_RejectsSuccessfully_WhenReasonProvided()
        {
            // Arrange
            var claim = BuildClaim("officer-001", ClaimStatus.UnderReview);
            _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Claim>()))
                          .ReturnsAsync((Domain.Entities.Claim c) => c);
            _policyRepoMock.Setup(r => r.GetByIdAsync(10))
                           .ReturnsAsync(new Policy { PolicyId = 10, PolicyNumber = "POL-001" });
            _claimDocRepoMock.Setup(r => r.GetByClaimIdAsync(1))
                             .ReturnsAsync(new List<ClaimDocument>());
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((ApplicationUser?)null);

            var dto     = new ReviewClaimDTO { IsApproved = false, RejectionReason = "Insufficient documents" };
            var service = CreateService();

            // Act
            var result = await service.ReviewClaimAsync(1, "officer-001", dto);

            // Assert
            Assert.Equal("Rejected",               result.Status);
            Assert.Equal("Insufficient documents", result.RejectionReason);
        }
    }
}
