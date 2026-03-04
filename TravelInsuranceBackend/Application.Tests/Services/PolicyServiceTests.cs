using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Services
{
    public class PolicyServiceTests
    {
        private readonly Mock<IPolicyProductRepository> _productRepoMock = new();
        private readonly Mock<IPolicyRepository>        _policyRepoMock  = new();
        private readonly Mock<IUserRepository>          _userRepoMock    = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public PolicyServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private PolicyService CreateService() =>
            new(_productRepoMock.Object, _policyRepoMock.Object,
                _userRepoMock.Object,   _userManagerMock.Object);

        [Fact]
        public async Task GetAvailableProducts_ReturnsOnlyAvailableProducts()
        {
            // Arrange
            var products = new List<PolicyProduct>
            {
                new() { PolicyProductId = 1, PolicyName = "Plan A", Status = PolicyProductStatus.Available },
                new() { PolicyProductId = 2, PolicyName = "Plan B", Status = PolicyProductStatus.Inactive }
            };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            var service = CreateService();

            // Act
            var result = await service.GetAvailablePolicyProductsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Plan A", result[0].PolicyName);
        }

        [Fact]
        public async Task PurchasePolicy_ThrowsException_WhenProductIsNotAvailable()
        {
            // Arrange
            var product = new PolicyProduct { PolicyProductId = 1, Status = PolicyProductStatus.Inactive };
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var dto = new CreatePolicyDTO
            {
                PolicyProductId = 1,
                StartDate       = DateTime.UtcNow.Date.AddDays(1),
                EndDate         = DateTime.UtcNow.Date.AddDays(10),
                KycType         = "PAN",
                KycNumber       = "ABCDE1234F",
                TravellerName   = "John",
                TravellerAge    = 28,
                Destination     = "Rome"
            };

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.PurchasePolicyAsync("cust-001", dto));
            Assert.Equal("This policy product is not available.", ex.Message);
        }

        [Fact]
        public async Task MakePayment_UpdatesPolicyStatusToActive_WhenPolicyIsPendingPayment()
        {
            // Arrange
            var policy = new Policy
            {
                PolicyId      = 1,
                PolicyNumber  = "POL-001",
                CustomerId    = "cust-001",
                PremiumAmount = 1200m,
                Status        = PolicyStatus.PendingPayment
            };
            _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Policy>()))
                           .ReturnsAsync((Policy p) => p);

            var service = CreateService();
            var dto     = new PolicyPaymentDTO { PolicyId = 1, PaymentMethod = "Card" };

            // Act
            var result = await service.MakePaymentAsync("cust-001", dto);

            // Assert
            Assert.Equal("Success", result.Status);
            Assert.Equal(1200m,     result.AmountPaid);
            _policyRepoMock.Verify(r => r.UpdateAsync(It.Is<Policy>(p =>
                p.Status == PolicyStatus.Active)), Times.Once);
        }
    }
}
