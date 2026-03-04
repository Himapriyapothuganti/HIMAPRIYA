using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities
{
    public class PolicyProductEntityTests
    {
        [Fact]
        public void PolicyProduct_DefaultStatus_IsAvailable()
        {
            // Arrange & Act
            var product = new PolicyProduct();

            // Assert
            Assert.Equal(PolicyProductStatus.Available, product.Status);
        }

        [Fact]
        public void PolicyProduct_CanAssignTenureAndPremiumFields()
        {
            // Arrange & Act
            var product = new PolicyProduct
            {
                PolicyProductId = 1,
                PolicyName      = "Global Shield Gold",
                PolicyType      = "Single Trip",
                PlanTier        = "Gold",
                CoverageDetails = "Medical, Baggage, Trip Cancellation",
                CoverageLimit   = 500000m,
                BasePremium     = 2000m,
                Tenure          = 30,
                ClaimLimit      = 100000m,
                DestinationZone = "Asia",
                Status          = PolicyProductStatus.Available
            };

            // Assert
            Assert.Equal(30,        product.Tenure);
            Assert.Equal(2000m,     product.BasePremium);
            Assert.Equal(100000m,   product.ClaimLimit);
            Assert.Equal("Gold",    product.PlanTier);
        }

        [Fact]
        public void PolicyProduct_InactiveStatus_CanBeSet()
        {
            // Arrange
            var product = new PolicyProduct();

            // Act
            product.Status = PolicyProductStatus.Inactive;

            // Assert
            Assert.Equal(PolicyProductStatus.Inactive, product.Status);
        }
    }
}
