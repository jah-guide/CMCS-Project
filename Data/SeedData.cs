using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roleNames = { "Lecturer", "Coordinator", "Manager" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Statuses
            if (!context.Statuses.Any())
            {
                context.Statuses.AddRange(
                    new Status { StatusName = "Submitted", Description = "Claim submitted by lecturer" },
                    new Status { StatusName = "ApprovedByCoordinator", Description = "Approved by programme coordinator" },
                    new Status { StatusName = "ApprovedByManager", Description = "Approved by academic manager" },
                    new Status { StatusName = "Rejected", Description = "Claim rejected" },
                    new Status { StatusName = "Paid", Description = "Claim has been paid" }
                );
                context.SaveChanges();
            }

            // Create default Coordinator
            var coordinatorEmail = "coordinator@cmcs.com";
            var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);
            if (coordinatorUser == null)
            {
                coordinatorUser = new ApplicationUser
                {
                    UserName = coordinatorEmail,
                    Email = coordinatorEmail,
                    FirstName = "John",
                    LastName = "Coordinator",
                    HourlyRate = 0,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(coordinatorUser, "Coordinator123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
                }
            }

            // Create default Manager
            var managerEmail = "manager@cmcs.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FirstName = "Sarah",
                    LastName = "Manager",
                    HourlyRate = 0,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(managerUser, "Manager123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }
        }
    }
}