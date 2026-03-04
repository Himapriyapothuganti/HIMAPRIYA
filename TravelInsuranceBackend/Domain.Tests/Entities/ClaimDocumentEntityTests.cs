using Domain.Entities;

namespace Domain.Tests.Entities
{
    public class ClaimDocumentEntityTests
    {
        [Fact]
        public void ClaimDocument_CanAssignFileProperties()
        {
            // Arrange & Act
            var doc = new ClaimDocument
            {
                ClaimDocumentId = 1,
                ClaimId         = 5,
                FileName        = "medical_bill.pdf",
                FilePath        = "wwwroot/claims/5/medical_bill.pdf",
                FileType        = "application/pdf",
                FileSize        = 204800L,
                UploadedAt      = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
            };

            // Assert
            Assert.Equal("medical_bill.pdf",               doc.FileName);
            Assert.Equal("application/pdf",                doc.FileType);
            Assert.Equal(204800L,                          doc.FileSize);
            Assert.Equal(5,                                doc.ClaimId);
        }

        [Fact]
        public void ClaimDocument_ClaimNavigation_DefaultsToNull()
        {
            // Arrange & Act
            var doc = new ClaimDocument();

            // Assert
            Assert.Null(doc.Claim);
        }

        [Fact]
        public void ClaimDocument_FileNameAndPath_DefaultToEmpty()
        {
            // Arrange & Act
            var doc = new ClaimDocument();

            // Assert
            Assert.Equal(string.Empty, doc.FileName);
            Assert.Equal(string.Empty, doc.FilePath);
            Assert.Equal(string.Empty, doc.FileType);
        }
    }
}
