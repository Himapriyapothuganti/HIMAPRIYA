using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class ClaimRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsClaim_ToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo  = new Infrastructure.Repositories.ClaimRepository(context);
            var claim = new Domain.Entities.Claim
            {
                PolicyId        = 1,
                CustomerId      = "cust-001",
                ClaimType       = "Medical",
                Description     = "Hospital bills",
                ClaimedAmount   = 5000m,
                ClaimsOfficerId = "officer-001",
                Status          = ClaimStatus.UnderReview
            };

            // Act
            var created = await repo.CreateAsync(claim);

            // Assert
            Assert.True(created.ClaimId > 0);
            Assert.Equal("Medical", created.ClaimType);
            Assert.Equal(1, await context.Claims.CountAsync());
        }

        [Fact]
        public async Task GetActiveClaimCountByOfficerAsync_ExcludesClosedAndRejected()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Claims.AddRange(
                new Domain.Entities.Claim { PolicyId = 1, CustomerId = "c1", ClaimsOfficerId = "officer-001", Status = ClaimStatus.UnderReview,      ClaimType = "X", Description = "D" },
                new Domain.Entities.Claim { PolicyId = 2, CustomerId = "c2", ClaimsOfficerId = "officer-001", Status = ClaimStatus.Approved,          ClaimType = "X", Description = "D" },
                new Domain.Entities.Claim { PolicyId = 3, CustomerId = "c3", ClaimsOfficerId = "officer-001", Status = ClaimStatus.Closed,            ClaimType = "X", Description = "D" },
                new Domain.Entities.Claim { PolicyId = 4, CustomerId = "c4", ClaimsOfficerId = "officer-001", Status = ClaimStatus.Rejected,          ClaimType = "X", Description = "D" }
            );
            await context.SaveChangesAsync();
            var repo = new Infrastructure.Repositories.ClaimRepository(context);

            // Act
            var activeCount = await repo.GetActiveClaimCountByOfficerAsync("officer-001");

            // Assert  (Closed + Rejected excluded → 2 active)
            Assert.Equal(2, activeCount);
        }

        [Fact]
        public async Task GetByOfficerIdAsync_ReturnsOnlyAssignedClaims()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Claims.AddRange(
                new Domain.Entities.Claim { PolicyId = 1, CustomerId = "c1", ClaimsOfficerId = "officer-001", ClaimType = "A", Description = "D" },
                new Domain.Entities.Claim { PolicyId = 2, CustomerId = "c2", ClaimsOfficerId = "officer-001", ClaimType = "B", Description = "D" },
                new Domain.Entities.Claim { PolicyId = 3, CustomerId = "c3", ClaimsOfficerId = "officer-999", ClaimType = "C", Description = "D" }
            );
            await context.SaveChangesAsync();
            var repo = new Infrastructure.Repositories.ClaimRepository(context);

            // Act
            var result = await repo.GetByOfficerIdAsync("officer-001");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("officer-001", c.ClaimsOfficerId));
        }
    }
}
