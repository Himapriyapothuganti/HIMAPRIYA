using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities
{
    public class ClaimEntityTests
    {
        [Fact]
        public void Claim_DefaultStatus_IsSubmitted()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            Assert.Equal(ClaimStatus.Submitted, claim.Status);
        }

        [Fact]
        public void Claim_CanAssignClaimedAndApprovedAmount()
        {
            // Arrange & Act
            var claim = new Claim
            {
                ClaimId        = 1,
                PolicyId       = 10,
                CustomerId     = "cust-001",
                ClaimType      = "Medical",
                Description    = "Hospital bills during trip",
                ClaimedAmount  = 5000.00m,
                ApprovedAmount = 4500.00m,
                Status         = ClaimStatus.Approved
            };

            // Assert
            Assert.Equal(5000.00m,            claim.ClaimedAmount);
            Assert.Equal(4500.00m,            claim.ApprovedAmount);
            Assert.Equal(ClaimStatus.Approved, claim.Status);
        }

        [Fact]
        public void Claim_RejectionReason_DefaultsToNull()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            Assert.Null(claim.RejectionReason);
            Assert.Null(claim.ApprovedAmount);
            Assert.Null(claim.ReviewedAt);
        }
    }
}
