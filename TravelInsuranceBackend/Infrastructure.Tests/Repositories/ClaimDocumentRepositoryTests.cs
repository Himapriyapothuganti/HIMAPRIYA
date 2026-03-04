using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    public class ClaimDocumentRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_PersistsDocument_ToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repo = new ClaimDocumentRepository(context);
            var doc  = new ClaimDocument
            {
                ClaimId    = 1,
                FileName   = "receipt.pdf",
                FilePath   = "wwwroot/claims/1/receipt.pdf",
                FileType   = "application/pdf",
                FileSize   = 102400L,
                UploadedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(doc);
            await repo.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await context.ClaimDocuments.CountAsync());
        }

        [Fact]
        public async Task GetByClaimIdAsync_ReturnsOnlyMatchingDocuments()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.ClaimDocuments.AddRange(
                new ClaimDocument { ClaimId = 1, FileName = "doc1.pdf", FilePath = "/claims/1/a.pdf", FileType = "pdf", UploadedAt = DateTime.UtcNow },
                new ClaimDocument { ClaimId = 1, FileName = "doc2.pdf", FilePath = "/claims/1/b.pdf", FileType = "pdf", UploadedAt = DateTime.UtcNow },
                new ClaimDocument { ClaimId = 2, FileName = "doc3.pdf", FilePath = "/claims/2/c.pdf", FileType = "pdf", UploadedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            var repo = new ClaimDocumentRepository(context);

            // Act
            var result = await repo.GetByClaimIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, d => Assert.Equal(1, d.ClaimId));
        }
    }
}
