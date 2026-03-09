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

       
        //Tests the CreateUser endpoint when a valid user creation request is made.
        // It verifies that if the admin service successfully creates an agent,
        // the controller returns a 200 OK response with the user details.
       
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

       
        /// Tests the CreateUser endpoint for a failure scenario, such as a duplicate email.
        /// It ensures that when the underlying service throws an Exception,
        /// the controller properly catches it or returns a 400 Bad Request.
       
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

        /// <summary>
        /// Tests the GetDashboard endpoint to verify it correctly fetches admin metrics.
        /// It ensures that when the service returns dashboard statistics,
        /// the controller returns them with a 200 OK status.
        /// </summary>
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
