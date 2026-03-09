using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class PolicyControllerTests
    {
        private readonly Mock<IPolicyService> _policyServiceMock = new();

        private PolicyController CreateController(string? userId = "cust-001")
        {
            var controller = new PolicyController(_policyServiceMock.Object);
            if (userId != null)
            {
                var claims = new[] { new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, userId) };
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims))
                    }
                };
            }
            return controller;
        }

        // Fetch products
        [Fact]
        public async Task GetAvailableProducts_ReturnsOk_WithProductList()
        {
            // Arrange
            var products = new List<PolicyProductResponseDTO>
            {
                new() { PolicyProductId = 1, PolicyName = "Plan A" },
                new() { PolicyProductId = 2, PolicyName = "Plan B" }
            };
            _policyServiceMock.Setup(s => s.GetAvailablePolicyProductsAsync())
                              .ReturnsAsync(products);

            var controller = CreateController();

            // Act
            var result = await controller.GetAvailableProducts() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // Purchase fails if product invalid
        [Fact]
        public async Task PurchasePolicy_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            _policyServiceMock.Setup(s => s.PurchasePolicyAsync(It.IsAny<string>(), It.IsAny<CreatePolicyDTO>()))
                              .ThrowsAsync(new Exception("This policy product is not available."));

            var dto        = new CreatePolicyDTO();
            var controller = CreateController();

            // Act
            var result = await controller.PurchasePolicy(dto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        // Get policies for current user
        [Fact]
        public async Task GetMyPolicies_ReturnsOk_WithPoliciesList()
        {
            // Arrange
            var policies = new List<PolicyResponseDTO>
            {
                new() { PolicyId = 1, PolicyNumber = "POL-001" }
            };
            _policyServiceMock.Setup(s => s.GetMyPoliciesAsync("cust-001"))
                              .ReturnsAsync(policies);

            var controller = CreateController("cust-001");

            // Act
            var result = await controller.GetMyPolicies() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
