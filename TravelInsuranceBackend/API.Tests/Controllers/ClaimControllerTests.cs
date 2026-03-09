using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers
{
    public class ClaimControllerTests
    {
        private readonly Mock<IClaimService> _claimServiceMock = new();

        private ClaimController CreateController(string userId = "cust-001")
        {
            var controller = new ClaimController(_claimServiceMock.Object);
            var claims     = new[] { new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, userId) };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims))
                }
            };
            return controller;
        }

        // Get customer's claims
        [Fact]
        public async Task GetMyClaims_ReturnsOk_WithClaimsList()
        {
            // Arrange
            var claims = new List<ClaimResponseDTO>
            {
                new() { ClaimId = 1, ClaimType = "Medical", Status = "UnderReview" }
            };
            _claimServiceMock.Setup(s => s.GetMyClaimsAsync("cust-001"))
                             .ReturnsAsync(claims);

            var controller = CreateController("cust-001");

            // Act
            var result = await controller.GetMyClaims() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // Prevent officer from reviewing others' claims
        [Fact]
        public async Task ReviewClaim_ReturnsBadRequest_WhenOfficerNotAssigned()
        {
            // Arrange
            _claimServiceMock.Setup(s => s.ReviewClaimAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ReviewClaimDTO>()))
                             .ThrowsAsync(new Exception("You are not assigned to this claim."));

            var dto        = new ReviewClaimDTO { IsApproved = true, ApprovedAmount = 1000m };
            var controller = CreateController("officer-999");

            // Act
            var result = await controller.ReviewClaim(1, dto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        // Admin gets all claims
        [Fact]
        public async Task GetAllClaims_ReturnsOk_WithAllClaims()
        {
            // Arrange
            var allClaims = new List<ClaimResponseDTO>
            {
                new() { ClaimId = 1 },
                new() { ClaimId = 2 }
            };
            _claimServiceMock.Setup(s => s.GetAllClaimsAsync()).ReturnsAsync(allClaims);

            var controller = CreateController();

            // Act
            var result = await controller.GetAllClaims() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
