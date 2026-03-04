using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class AgentServiceTests
    {
        private readonly Mock<IPolicyRepository>            _policyRepoMock  = new();
        private readonly Mock<IPolicyProductRepository>     _productRepoMock = new();
        private readonly Mock<IClaimRepository>             _claimRepoMock   = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public AgentServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private AgentService CreateService() =>
            new(_policyRepoMock.Object, _productRepoMock.Object,
                _claimRepoMock.Object,  _userManagerMock.Object);

        [Fact]
        public async Task GetDashboard_ThrowsException_WhenAgentNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByIdAsync("agent-999"))
                            .ReturnsAsync((ApplicationUser?)null);

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetDashboardAsync("agent-999"));
            Assert.Equal("Agent not found.", ex.Message);
        }

        [Fact]
        public async Task GetDashboard_ReturnsCorrectActivePoliciesCount()
        {
            // Arrange
            var agent = new ApplicationUser { Id = "agent-001", FullName = "Tom Agent", Email = "tom@example.com" };
            _userManagerMock.Setup(m => m.FindByIdAsync("agent-001")).ReturnsAsync(agent);

            var policies = new List<Policy>
            {
                new() { PolicyId = 1, AgentId = "agent-001", Status = PolicyStatus.Active,         PremiumAmount = 1000m },
                new() { PolicyId = 2, AgentId = "agent-001", Status = PolicyStatus.Active,         PremiumAmount = 2000m },
                new() { PolicyId = 3, AgentId = "agent-001", Status = PolicyStatus.PendingPayment, PremiumAmount = 500m  }
            };
            _policyRepoMock.Setup(r => r.GetByAgentIdAsync("agent-001")).ReturnsAsync(policies);

            // For each policy in MapPoliciesAsync, needs product + customer
            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(new PolicyProduct { PolicyName = "Plan X" });
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync(agent);

            var service = CreateService();

            // Act
            var result = await service.GetDashboardAsync("agent-001");

            // Assert
            Assert.Equal(2, result.ActivePolicies);
            Assert.Equal(3, result.TotalPoliciesAssigned);
            Assert.Equal(3000m * 0.05m, result.TotalCommissionEarned);
        }

        [Fact]
        public async Task GetPolicyDetail_ThrowsException_WhenPolicyNotAssignedToAgent()
        {
            // Arrange
            var policy = new Policy { PolicyId = 1, AgentId = "agent-001" };
            _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetPolicyDetailAsync("agent-999", 1));
            Assert.Equal("You are not assigned to this policy.", ex.Message);
        }
    }
}
