using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities
{
    public class PolicyEntityTests
    {
        [Fact]
        public void Policy_DefaultStatus_IsPendingPayment()
        {
            // Arrange & Act
            var policy = new Policy();

            // Assert
            Assert.Equal(PolicyStatus.PendingPayment, policy.Status);
        }

        [Fact]
        public void Policy_CanAssignAllProperties()
        {
            // Arrange
            var start = DateTime.UtcNow.Date.AddDays(1);
            var end   = start.AddDays(10);

            // Act
            var policy = new Policy
            {
                PolicyId        = 1,
                PolicyProductId = 2,
                PolicyNumber    = "POL-20240101-ABCDEF",
                CustomerId      = "cust-001",
                AgentId         = "agent-001",
                Destination     = "Paris",
                PolicyType      = "Single Trip",
                PlanTier        = "Gold",
                TravellerName   = "John Doe",
                TravellerAge    = 30,
                PassportNumber  = "P1234567",
                KycType         = "PAN",
                KycNumber       = "ABCDE1234F",
                PremiumAmount   = 1500.00m,
                StartDate       = start,
                EndDate         = end,
                Status          = PolicyStatus.Active
            };

            // Assert
            Assert.Equal("cust-001",          policy.CustomerId);
            Assert.Equal("Paris",             policy.Destination);
            Assert.Equal(1500.00m,            policy.PremiumAmount);
            Assert.Equal(PolicyStatus.Active, policy.Status);
            Assert.Equal("PAN",               policy.KycType);
        }

        [Fact]
        public void Policy_Claims_DefaultsToEmptyCollection()
        {
            // Arrange & Act
            var policy = new Policy();

            // Assert
            Assert.NotNull(policy.Claims);
            Assert.Empty(policy.Claims);
        }
    }
}
