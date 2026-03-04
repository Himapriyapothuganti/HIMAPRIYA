using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>>  _userManagerMock;
        private readonly Mock<IPolicyProductRepository>      _policyProductRepoMock = new();
        private readonly Mock<IPolicyRepository>             _policyRepoMock        = new();
        private readonly Mock<IUserRepository>               _userRepoMock          = new();
        private readonly Mock<IClaimRepository>              _claimRepoMock         = new();

        public AdminServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private AdminService CreateService() =>
            new(_userManagerMock.Object, _policyProductRepoMock.Object,
                _policyRepoMock.Object,  _userRepoMock.Object, _claimRepoMock.Object);

        [Fact]
        public async Task CreateUser_ThrowsException_WhenRoleIsCustomer()
        {
            // Arrange
            var dto = new CreateUserDTO
            {
                FullName = "Jane User",
                Email    = "jane@example.com",
                Password = "Pass@123",
                Role     = UserRole.Customer
            };
            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateUserAsync(dto));
            Assert.Equal("You can only create Agent or ClaimsOfficer accounts.", ex.Message);
        }

        [Fact]
        public async Task CreatePolicyProduct_ReturnsMappedDTO()
        {
            // Arrange
            var dto = new CreatePolicyProductDTO
            {
                PolicyName      = "Gold Shield",
                PolicyType      = "Single Trip",
                PlanTier        = "Gold",
                CoverageDetails = "Medical + Baggage",
                CoverageLimit   = 500000m,
                BasePremium     = 2000m,
                Tenure          = 30,
                ClaimLimit      = 100000m,
                DestinationZone = "Asia"
            };

            var createdProduct = new PolicyProduct
            {
                PolicyProductId = 1,
                PolicyName      = dto.PolicyName,
                PolicyType      = dto.PolicyType,
                PlanTier        = dto.PlanTier,
                CoverageDetails = dto.CoverageDetails,
                CoverageLimit   = dto.CoverageLimit,
                BasePremium     = dto.BasePremium,
                Tenure          = dto.Tenure,
                ClaimLimit      = dto.ClaimLimit,
                DestinationZone = dto.DestinationZone,
                Status          = PolicyProductStatus.Available
            };
            _policyProductRepoMock.Setup(r => r.CreateAsync(It.IsAny<PolicyProduct>()))
                                  .ReturnsAsync(createdProduct);

            var service = CreateService();

            // Act
            var result = await service.CreatePolicyProductAsync(dto);

            // Assert
            Assert.Equal("Gold Shield",              result.PolicyName);
            Assert.Equal("Available",                result.Status);
            Assert.Equal(2000m,                      result.BasePremium);
        }

        [Fact]
        public async Task ActivateDeactivateUser_ReturnsCorrectMessage()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-001", IsActive = true };
            _userRepoMock.Setup(r => r.GetByIdAsync("user-001")).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ApplicationUser>()))
                         .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var activateMsg   = await service.ActivateDeactivateUserAsync("user-001", true);
            var deactivateMsg = await service.ActivateDeactivateUserAsync("user-001", false);

            // Assert
            Assert.Equal("User activated successfully.",   activateMsg);
            Assert.Equal("User deactivated successfully.", deactivateMsg);
        }
    }
}
