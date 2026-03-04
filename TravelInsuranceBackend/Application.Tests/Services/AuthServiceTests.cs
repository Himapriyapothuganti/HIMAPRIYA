using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConfiguration>               _configMock = new();

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Set up JWT config sections
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(s => s["Key"]).Returns("SuperSecretKey12345678901234567890");
            jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
            jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
            jwtSection.Setup(s => s["ExpiryMinutes"]).Returns("60");
            _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        }

        private AuthService CreateService() =>
            new(_userManagerMock.Object, _configMock.Object);

        [Fact]
        public async Task Register_ThrowsException_WhenEmailAlreadyExists()
        {
            // Arrange
            var existing = new ApplicationUser { Email = "test@example.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync("test@example.com"))
                            .ReturnsAsync(existing);

            var dto = new RegisterDTO
            {
                FullName        = "John Doe",
                Email           = "test@example.com",
                Password        = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(dto));
            Assert.Equal("A user with this email already exists.", ex.Message);
        }

        [Fact]
        public async Task Login_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByEmailAsync("unknown@example.com"))
                            .ReturnsAsync((ApplicationUser?)null);

            var dto     = new LoginDTO { Email = "unknown@example.com", Password = "Password123!" };
            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(dto));
            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task Login_ThrowsException_WhenAccountIsDeactivated()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Email    = "deactivated@example.com",
                IsActive = false,
                FullName = "Inactive User"
            };
            _userManagerMock.Setup(m => m.FindByEmailAsync("deactivated@example.com"))
                            .ReturnsAsync(user);

            var dto     = new LoginDTO { Email = "deactivated@example.com", Password = "Password123!" };
            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(dto));
            Assert.Equal("Your account has been deactivated. Contact admin.", ex.Message);
        }
    }
}
