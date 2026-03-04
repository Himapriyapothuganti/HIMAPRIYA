using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class PolicyRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsPolicy_ToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo   = new Infrastructure.Repositories.PolicyRepository(context);
            var policy = new Policy
            {
                PolicyProductId = 1,
                PolicyNumber    = "POL-TEST-001",
                CustomerId      = "cust-001",
                Destination     = "Paris",
                PolicyType      = "Single Trip",
                PlanTier        = "Gold",
                TravellerName   = "John",
                TravellerAge    = 30,
                PremiumAmount   = 1000m,
                StartDate       = DateTime.UtcNow.AddDays(1),
                EndDate         = DateTime.UtcNow.AddDays(11),
                KycType         = "PAN",
                KycNumber       = "ABCDE1234F"
            };

            // Act
            var created = await repo.CreateAsync(policy);

            // Assert
            Assert.True(created.PolicyId > 0);
            Assert.Equal("POL-TEST-001", created.PolicyNumber);
            Assert.Equal(1, await context.Policies.CountAsync());
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenPolicyNotFound()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo = new Infrastructure.Repositories.PolicyRepository(context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => repo.GetByIdAsync(999));
            Assert.Equal("Policy not found.", ex.Message);
        }

        [Fact]
        public async Task GetByCustomerIdAsync_ReturnsOnlyCustomerPolicies()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Policies.AddRange(
                new Policy { PolicyProductId = 1, CustomerId = "cust-001", PolicyNumber = "POL-001", PolicyType = "Single Trip", PlanTier = "Gold", TravellerName = "A", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), KycType = "PAN", KycNumber = "A" },
                new Policy { PolicyProductId = 1, CustomerId = "cust-001", PolicyNumber = "POL-002", PolicyType = "Single Trip", PlanTier = "Gold", TravellerName = "B", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), KycType = "PAN", KycNumber = "B" },
                new Policy { PolicyProductId = 1, CustomerId = "cust-999", PolicyNumber = "POL-003", PolicyType = "Single Trip", PlanTier = "Gold", TravellerName = "C", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), KycType = "PAN", KycNumber = "C" }
            );
            await context.SaveChangesAsync();
            var repo = new Infrastructure.Repositories.PolicyRepository(context);

            // Act
            var result = await repo.GetByCustomerIdAsync("cust-001");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("cust-001", p.CustomerId));
        }
    }
}
