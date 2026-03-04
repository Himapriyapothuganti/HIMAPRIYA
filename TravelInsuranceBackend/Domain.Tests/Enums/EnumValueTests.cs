using Domain.Enums;

namespace Domain.Tests.Enums
{
    public class EnumValueTests
    {
        [Fact]
        public void ClaimStatus_HasAllExpectedValues()
        {
            // Arrange
            var expectedValues = new[]
            {
                ClaimStatus.Submitted,
                ClaimStatus.UnderReview,
                ClaimStatus.PendingDocuments,
                ClaimStatus.Approved,
                ClaimStatus.Rejected,
                ClaimStatus.PaymentProcessed,
                ClaimStatus.Closed
            };

            // Act
            var actualValues = Enum.GetValues<ClaimStatus>();

            // Assert
            Assert.Equal(7, actualValues.Length);
            foreach (var v in expectedValues)
                Assert.Contains(v, actualValues);
        }

        [Fact]
        public void PolicyStatus_HasAllExpectedValues()
        {
            // Arrange
            var expectedValues = new[]
            {
                PolicyStatus.PendingPayment,
                PolicyStatus.Active,
                PolicyStatus.Expired,
                PolicyStatus.Cancelled
            };

            // Act
            var actualValues = Enum.GetValues<PolicyStatus>();

            // Assert
            Assert.Equal(4, actualValues.Length);
            foreach (var v in expectedValues)
                Assert.Contains(v, actualValues);
        }

        [Fact]
        public void PolicyProductStatus_HasExpectedValues()
        {
            // Arrange & Act
            var actualValues = Enum.GetValues<PolicyProductStatus>();

            // Assert
            Assert.Equal(2, actualValues.Length);
            Assert.Contains(PolicyProductStatus.Available, actualValues);
            Assert.Contains(PolicyProductStatus.Inactive,  actualValues);
        }

        [Fact]
        public void UserRole_ConstantsMatchExpectedStrings()
        {
            // Arrange & Act & Assert
            Assert.Equal("Admin",          UserRole.Admin);
            Assert.Equal("Agent",          UserRole.Agent);
            Assert.Equal("Customer",       UserRole.Customer);
            Assert.Equal("ClaimsOfficer",  UserRole.ClaimsOfficer);
        }
    }
}
