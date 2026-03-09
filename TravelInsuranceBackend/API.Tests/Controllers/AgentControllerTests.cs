using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class AgentControllerTests
    {
        private readonly Mock<IAgentService> _agentServiceMock = new();

        private AgentController CreateController(string agentId = "agent-001")
        {
            var controller = new AgentController(_agentServiceMock.Object);
            var claims     = new[] { new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, agentId) };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims))
                }
            };
            return controller;
        }

        // Agent gets their dashboard
        [Fact]
        public async Task GetDashboard_ReturnsOk_WhenAgentExists()
        {
            // Arrange
            var dashboard = new AgentDashboardDTO
            {
                AgentId       = "agent-001",
                AgentName     = "Tom Agent",
                ActivePolicies = 3
            };
            _agentServiceMock.Setup(s => s.GetDashboardAsync("agent-001"))
                             .ReturnsAsync(dashboard);

            var controller = CreateController("agent-001");

            // Act
            var result = await controller.GetDashboard() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // Agent gets their assigned policies
        [Fact]
        public async Task GetAssignedPolicies_ReturnsOk_WithList()
        {
            // Arrange
            var policies = new List<PolicyResponseDTO>
            {
                new() { PolicyId = 1, PolicyNumber = "POL-001" }
            };
            _agentServiceMock.Setup(s => s.GetAssignedPoliciesAsync("agent-001"))
                             .ReturnsAsync(policies);

            var controller = CreateController("agent-001");

            // Act
            var result = await controller.GetAssignedPolicies() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // Handle agent not found
        [Fact]
        public async Task GetDashboard_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            _agentServiceMock.Setup(s => s.GetDashboardAsync(It.IsAny<string>()))
                             .ThrowsAsync(new Exception("Agent not found."));

            var controller = CreateController("agent-999");

            // Act
            var result = await controller.GetDashboard() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }
    }
}
