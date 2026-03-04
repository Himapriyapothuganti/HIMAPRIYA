using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class PolicyProductRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.PolicyProducts.AddRange(
                new PolicyProduct { PolicyName = "Plan A", PolicyType = "Single Trip", PlanTier = "Silver", DestinationZone = "Asia",   BasePremium = 1000m, CoverageLimit = 500000m, ClaimLimit = 100000m, Tenure = 30 },
                new PolicyProduct { PolicyName = "Plan B", PolicyType = "Multi-Trip",  PlanTier = "Gold",   DestinationZone = "Global", BasePremium = 2000m, CoverageLimit = 700000m, ClaimLimit = 200000m, Tenure = 60 }
            );
            await context.SaveChangesAsync();
            var repo = new Infrastructure.Repositories.PolicyProductRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenProductNotFound()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo = new Infrastructure.Repositories.PolicyProductRepository(context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => repo.GetByIdAsync(999));
            Assert.Equal("Policy product not found.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_PersistsProductWithAvailableStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo    = new Infrastructure.Repositories.PolicyProductRepository(context);
            var product = new PolicyProduct
            {
                PolicyName      = "Premium Plan",
                PolicyType      = "Family",
                PlanTier        = "Premium",
                CoverageDetails = "Full coverage",
                CoverageLimit   = 1000000m,
                BasePremium     = 3000m,
                Tenure          = 90,
                ClaimLimit      = 500000m,
                DestinationZone = "Worldwide",
                Status          = PolicyProductStatus.Available
            };

            // Act
            var created = await repo.CreateAsync(product);

            // Assert
            Assert.True(created.PolicyProductId > 0);
            Assert.Equal(PolicyProductStatus.Available, created.Status);
        }
    }
}
