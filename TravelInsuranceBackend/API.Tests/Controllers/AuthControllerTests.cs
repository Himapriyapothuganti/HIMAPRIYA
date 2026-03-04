using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();

        private AuthController CreateController() =>
            new(_authServiceMock.Object);

        [Fact]
        public async Task Register_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var response = new AuthResponseDTO
            {
                Token    = "fake-jwt-token",
                Email    = "john@example.com",
                FullName = "John Doe",
                Role     = "Customer",
                Expiry   = DateTime.UtcNow.AddHours(1)
            };
            _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
                            .ReturnsAsync(response);

            var dto        = new RegisterDTO { FullName = "John Doe", Email = "john@example.com", Password = "Pass@123", ConfirmPassword = "Pass@123" };
            var controller = CreateController();

            // Act
            var result = await controller.Register(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
            Assert.Equal(response, result.Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
                            .ThrowsAsync(new Exception("Invalid email or password."));

            var dto        = new LoginDTO { Email = "wrong@email.com", Password = "WrongPass" };
            var controller = CreateController();

            // Act
            var result = await controller.Login(dto) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var response = new AuthResponseDTO { Token = "jwt", Email = "user@example.com", Role = "Customer" };
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>())).ReturnsAsync(response);

            var dto        = new LoginDTO { Email = "user@example.com", Password = "Pass@123" };
            var controller = CreateController();

            // Act
            var result = await controller.Login(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
