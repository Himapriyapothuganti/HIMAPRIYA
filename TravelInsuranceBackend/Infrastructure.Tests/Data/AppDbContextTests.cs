using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data
{
    public class AppDbContextTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrievePolicy()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var policy = new Policy
            {
                PolicyProductId = 1,
                PolicyNumber    = "POL-CONTEXT-001",
                CustomerId      = "cust-001",
                Destination     = "Tokyo",
                PolicyType      = "Single Trip",
                PlanTier        = "Silver",
                TravellerName   = "Alice",
                TravellerAge    = 27,
                PremiumAmount   = 1500m,
                StartDate       = DateTime.UtcNow.AddDays(5),
                EndDate         = DateTime.UtcNow.AddDays(15),
                KycType         = "Aadhaar",
                KycNumber       = "123456789012"
            };

            // Act
            context.Policies.Add(policy);
            await context.SaveChangesAsync();

            var retrieved = await context.Policies.FirstOrDefaultAsync(p => p.PolicyNumber == "POL-CONTEXT-001");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Tokyo",   retrieved!.Destination);
            Assert.Equal(1500m,     retrieved.PremiumAmount);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveClaim()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var claim = new Domain.Entities.Claim
            {
                PolicyId        = 1,
                CustomerId      = "cust-001",
                ClaimType       = "Baggage",
                Description     = "Lost luggage",
                ClaimedAmount   = 2500m,
                ClaimsOfficerId = "officer-001",
                Status          = ClaimStatus.UnderReview
            };

            // Act
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            var retrieved = await context.Claims.FirstOrDefaultAsync(c => c.ClaimType == "Baggage");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(2500m,                 retrieved!.ClaimedAmount);
            Assert.Equal(ClaimStatus.UnderReview, retrieved.Status);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrievePolicyProduct()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var product = new PolicyProduct
            {
                PolicyName      = "World Explorer Premium",
                PolicyType      = "Multi-Trip",
                PlanTier        = "Premium",
                CoverageDetails = "Full Coverage",
                CoverageLimit   = 1000000m,
                BasePremium     = 5000m,
                Tenure          = 365,
                ClaimLimit      = 500000m,
                DestinationZone = "Worldwide",
                Status          = PolicyProductStatus.Available
            };

            // Act
            context.PolicyProducts.Add(product);
            await context.SaveChangesAsync();

            var retrieved = await context.PolicyProducts.FirstOrDefaultAsync(p => p.PolicyName == "World Explorer Premium");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(5000m,                        retrieved!.BasePremium);
            Assert.Equal(PolicyProductStatus.Available, retrieved.Status);
        }
    }
}
