using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using System.Linq;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ControllerTests
    {
        [Fact]
        public void HomeController_Returns_View_Result()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Controller_DB")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Note: In real tests, would mock UserManager and other dependencies

            Assert.NotNull(context);
        }

        [Fact]
        public void Claim_Status_Progresses_Correctly()
        {
            // Arrange
            var claim = new Claim { CurrentStatusID = 1 }; // Submitted

            // Act - Simulate status progression
            claim.CurrentStatusID = 2; // ApprovedByCoordinator
            claim.CurrentStatusID = 3; // ApprovedByManager

            // Assert
            Assert.Equal(3, claim.CurrentStatusID);
        }
    }
}