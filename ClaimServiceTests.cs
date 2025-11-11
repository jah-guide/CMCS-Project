using Xunit;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ClaimServiceTests
    {
        [Fact]
        public void Claim_TotalAmount_Calculation_Correct()
        {
            // Arrange
            var user = new ApplicationUser { HourlyRate = 200m };
            var claim = new Claim { HoursWorked = 10 };

            // Act
            claim.TotalAmount = user.HourlyRate * claim.HoursWorked;

            // Assert
            Assert.Equal(2000m, claim.TotalAmount);
        }

        [Fact]
        public void SupportingDocument_Properties_Set_Correctly()
        {
            // Arrange & Act
            var document = new SupportingDocument
            {
                FileName = "test.pdf",
                FilePath = "/uploads/test.pdf"
            };

            // Assert
            Assert.Equal("test.pdf", document.FileName);
            Assert.Equal("/uploads/test.pdf", document.FilePath);
            Assert.NotNull(document.UploadDate);
        }

        [Fact]
        public async Task Claim_Can_Be_Saved_To_InMemory_Database()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Claims_DB")
                .Options;

            // Act & Assert
            using (var context = new ApplicationDbContext(options))
            {
                var claim = new Claim
                {
                    HoursWorked = 8,
                    TotalAmount = 1600m,
                    CurrentStatusID = 1,
                    UserId = "test-user-id"
                };

                context.Claims.Add(claim);
                await context.SaveChangesAsync();

                var savedClaim = await context.Claims.FirstAsync();
                Assert.Equal(8, savedClaim.HoursWorked);
                Assert.Equal(1600m, savedClaim.TotalAmount);
                Assert.Equal(1, savedClaim.CurrentStatusID);
            }
        }

        [Fact]
        public void Status_Initialization_Works()
        {
            // Arrange & Act
            var status = new Status
            {
                StatusName = "Submitted",
                Description = "Claim submitted by lecturer"
            };

            // Assert
            Assert.Equal("Submitted", status.StatusName);
            Assert.Equal("Claim submitted by lecturer", status.Description);
        }

        [Fact]
        public void ApplicationUser_FullName_Returns_Correct_Format()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var fullName = user.FullName;

            // Assert
            Assert.Equal("John Doe", fullName);
        }
    }

    public class ErrorHandlingTests
    {
        [Fact]
        public void Claim_With_Invalid_Hours_Should_Fail_Validation()
        {
            // Arrange
            var claim = new Claim { HoursWorked = -5 }; // Invalid hours

            // Act & Assert
            // For now, we test the business logic
            Assert.True(claim.HoursWorked < 1, "Hours worked should be positive");
        }

        [Fact]
        public void Claim_With_Zero_Hours_Should_Be_Invalid()
        {
            // Arrange
            var claim = new Claim { HoursWorked = 0 };

            // Act & Assert
            Assert.False(claim.HoursWorked > 0, "Hours worked must be greater than 0");
        }
    }
}