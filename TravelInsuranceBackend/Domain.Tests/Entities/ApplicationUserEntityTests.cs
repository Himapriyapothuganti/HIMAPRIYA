using Domain.Entities;

namespace Domain.Tests.Entities
{
    public class ApplicationUserEntityTests
    {
        [Fact]
        public void ApplicationUser_IsActiveByDefault()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.True(user.IsActive);
        }

        [Fact]
        public void ApplicationUser_CanAssignFullNameAndRole()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                FullName  = "Jane Smith",
                Role      = "Agent",
                IsActive  = true,
                Email     = "jane@example.com",
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            // Assert
            Assert.Equal("Jane Smith",  user.FullName);
            Assert.Equal("Agent",       user.Role);
            Assert.Equal("jane@example.com", user.Email);
        }

        [Fact]
        public void ApplicationUser_FullName_DefaultsToEmpty()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.Equal(string.Empty, user.FullName);
            Assert.Equal(string.Empty, user.Role);
        }
    }
}
