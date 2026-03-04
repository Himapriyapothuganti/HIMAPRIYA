using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _adminServiceMock = new();

        private AdminController CreateController() =>
            new(_adminServiceMock.Object);

        [Fact]
        public async Task CreateUser_ReturnsOk_WhenAgentCreated()
        {
            // Arrange
            var response = new UserResponseDTO
            {
                Id       = "agent-001",
                FullName = "New Agent",
                Email    = "agent@example.com",
                Role     = "Agent",
                IsActive = true
            };
            _adminServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDTO>()))
                             .ReturnsAsync(response);

            var dto        = new CreateUserDTO { FullName = "New Agent", Email = "agent@example.com", Password = "Pass@123", Role = "Agent" };
            var controller = CreateController();

            // Act
            var result = await controller.CreateUser(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            _adminServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDTO>()))
                             .ThrowsAsync(new Exception("A user with this email already exists."));

            var dto        = new CreateUserDTO { Email = "dupe@example.com", Role = "Agent" };
            var controller = CreateController();

            // Act
            var result = await controller.CreateUser(dto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task GetDashboard_ReturnsOk_WithDashboardData()
        {
            // Arrange
            var dashboard = new AdminDashboardDTO { TotalPolicies = 10, TotalUsers = 5 };
            _adminServiceMock.Setup(s => s.GetDashboardAsync()).ReturnsAsync(dashboard);

            var controller = CreateController();

            // Act
            var result = await controller.GetDashboard() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
