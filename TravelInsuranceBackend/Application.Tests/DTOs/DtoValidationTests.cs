using Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.Tests.DTOs
{
    public class DtoValidationTests
    {
        private static IList<ValidationResult> Validate(object dto)
        {
            var ctx     = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, ctx, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void RegisterDTO_IsInvalid_WhenEmailMissing()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                FullName        = "John Doe",
                Email           = "",          // missing
                Password        = "Pass@123",
                ConfirmPassword = "Pass@123"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(RegisterDTO.Email)));
        }

        [Fact]
        public void RegisterDTO_IsInvalid_WhenPasswordTooShort()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                FullName        = "John Doe",
                Email           = "john@example.com",
                Password        = "123",       // too short (min 6)
                ConfirmPassword = "123"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(RegisterDTO.Password)));
        }

        [Fact]
        public void RegisterDTO_IsValid_WhenAllRequiredFieldsProvided()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                FullName        = "Jane Doe",
                Email           = "jane@example.com",
                Password        = "Pass@1234",
                ConfirmPassword = "Pass@1234"
            };

            // Act
            var results = Validate(dto);

            // Assert
            Assert.Empty(results);
        }
    }
}
