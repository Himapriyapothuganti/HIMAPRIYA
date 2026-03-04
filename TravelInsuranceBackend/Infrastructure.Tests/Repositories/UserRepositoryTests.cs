using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectUser()
        {
            // Arrange
            var mockUM = CreateMockUserManager();
            var expected = new ApplicationUser { Id = "target-123", FullName = "Target" };
            mockUM.Setup(m => m.FindByIdAsync("target-123")).ReturnsAsync(expected);

            var repo = new UserRepository(mockUM.Object);

            // Act
            var user = await repo.GetByIdAsync("target-123");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("target-123", user.Id);
            Assert.Equal("Target", user.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var mockUM = CreateMockUserManager();
            mockUM.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

            var repo = new UserRepository(mockUM.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => repo.GetByIdAsync("missing"));
            Assert.Equal("User not found.", ex.Message);
        }

        [Fact]
        public async Task GetByRoleAsync_ReturnsUsersOfMatchingRole()
        {
            // Arrange
            var mockUM = CreateMockUserManager();
            var agents = new List<ApplicationUser>
            {
                new() { Id = "u1", FullName = "A", Role = UserRole.Agent },
                new() { Id = "u2", FullName = "B", Role = UserRole.Agent }
            };
            mockUM.Setup(m => m.GetUsersInRoleAsync(UserRole.Agent))
                  .ReturnsAsync(agents);

            var repo = new UserRepository(mockUM.Object);

            // Act
            var result = await repo.GetByRoleAsync(UserRole.Agent);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal(UserRole.Agent, a.Role));
        }
    }
}
