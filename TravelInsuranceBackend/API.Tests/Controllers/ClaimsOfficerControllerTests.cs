using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class ClaimsOfficerControllerTests
    {
        private readonly Mock<IClaimsOfficerService> _officerServiceMock = new();

        private ClaimsOfficerController CreateController(string officerId = "officer-001")
        {
            var controller = new ClaimsOfficerController(_officerServiceMock.Object);
            var claims     = new[] { new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, officerId) };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims))
                }
            };
            return controller;
        }

        // Get officer dashboard
        [Fact]
        public async Task GetDashboard_ReturnsOk_WhenOfficerExists()
        {
            // Arrange
            var dashboard = new OfficerDashboardDTO
            {
                OfficerId          = "officer-001",
                OfficerName        = "Sara",
                TotalAssignedClaims = 5
            };
            _officerServiceMock.Setup(s => s.GetDashboardAsync("officer-001"))
                               .ReturnsAsync(dashboard);

            var controller = CreateController("officer-001");

            // Act
            var result = await controller.GetDashboard() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // Handle invalid officer
        [Fact]
        public async Task GetDashboard_ReturnsBadRequest_WhenOfficerNotFound()
        {
            // Arrange
            _officerServiceMock.Setup(s => s.GetDashboardAsync(It.IsAny<string>()))
                               .ThrowsAsync(new Exception("Officer not found."));

            var controller = CreateController("bad-officer");

            // Act
            var result = await controller.GetDashboard() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }
    }
}
