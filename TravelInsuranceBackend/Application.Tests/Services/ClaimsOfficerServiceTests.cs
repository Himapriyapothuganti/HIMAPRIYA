using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class ClaimsOfficerServiceTests
    {
        private readonly Mock<IClaimRepository>             _claimRepoMock  = new();
        private readonly Mock<IPolicyRepository>            _policyRepoMock = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public ClaimsOfficerServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private ClaimsOfficerService CreateService() =>
            new(_claimRepoMock.Object, _policyRepoMock.Object, _userManagerMock.Object);

        [Fact]
        public async Task GetDashboard_ThrowsException_WhenOfficerNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByIdAsync("officer-999"))
                            .ReturnsAsync((ApplicationUser?)null);

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetDashboardAsync("officer-999"));
            Assert.Equal("Officer not found.", ex.Message);
        }

        [Fact]
        public async Task GetDashboard_CountsClaimsCorrectlyByStatus()
        {
            // Arrange
            var officer = new ApplicationUser
            {
                Id       = "officer-001",
                FullName = "Sara Officer",
                Email    = "sara@example.com"
            };
            _userManagerMock.Setup(m => m.FindByIdAsync("officer-001")).ReturnsAsync(officer);

            var claims = new List<Domain.Entities.Claim>
            {
                new() { ClaimId = 1, ClaimsOfficerId = "officer-001", Status = ClaimStatus.UnderReview,  PolicyId = 1, CustomerId = "c1" },
                new() { ClaimId = 2, ClaimsOfficerId = "officer-001", Status = ClaimStatus.UnderReview,  PolicyId = 2, CustomerId = "c2" },
                new() { ClaimId = 3, ClaimsOfficerId = "officer-001", Status = ClaimStatus.Approved,     PolicyId = 3, CustomerId = "c3", ApprovedAmount = 3000m },
                new() { ClaimId = 4, ClaimsOfficerId = "officer-001", Status = ClaimStatus.Rejected,     PolicyId = 4, CustomerId = "c4" }
            };
            _claimRepoMock.Setup(r => r.GetByOfficerIdAsync("officer-001")).ReturnsAsync(claims);

            // For each claim MapClaimsAsync needs a policy
            _policyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync(new Policy { PolicyNumber = "POL-X" });
            // customer / officer lookup returns null (handled gracefully)
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync(officer);

            var service = CreateService();

            // Act
            var result = await service.GetDashboardAsync("officer-001");

            // Assert
            Assert.Equal(4, result.TotalAssignedClaims);
            Assert.Equal(2, result.UnderReviewClaims);
            Assert.Equal(1, result.ApprovedClaims);
            Assert.Equal(1, result.RejectedClaims);
        }
    }
}
