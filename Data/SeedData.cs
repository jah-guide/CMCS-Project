// File: Data/SeedData.cs - CORRECTED VERSION
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

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Seed Roles - ADD HR ROLE
            string[] roleNames = { "Lecturer", "Coordinator", "Manager", "HR" };
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
                    new Status { StatusID = 1, StatusName = "Submitted", Description = "Claim submitted by lecturer" },
                    new Status { StatusID = 2, StatusName = "ApprovedByCoordinator", Description = "Approved by programme coordinator" },
                    new Status { StatusID = 3, StatusName = "ApprovedByManager", Description = "Approved by academic manager" },
                    new Status { StatusID = 4, StatusName = "Rejected", Description = "Claim rejected" },
                    new Status { StatusID = 5, StatusName = "Paid", Description = "Claim has been paid" }
                );
                await context.SaveChangesAsync();
            }

            // Create default HR User - FIXED
            var hrEmail = "hr@cmcs.com";
            var hrUser = await userManager.FindByEmailAsync(hrEmail);
            if (hrUser == null)
            {
                hrUser = new ApplicationUser
                {
                    UserName = hrEmail,
                    Email = hrEmail,
                    FirstName = "HR",
                    LastName = "Manager",
                    HourlyRate = 0,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(hrUser, "HR123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, "HR");
                    Console.WriteLine("HR user created successfully!");
                }
                else
                {
                    // Log errors
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"HR user creation error: {error.Description}");
                    }
                }
            }
            else
            {
                // Ensure HR user has the HR role
                if (!await userManager.IsInRoleAsync(hrUser, "HR"))
                {
                    await userManager.AddToRoleAsync(hrUser, "HR");
                }
            }

            // Create default Coordinator - FIXED
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
                    Console.WriteLine("Coordinator user created successfully!");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(coordinatorUser, "Coordinator"))
                {
                    await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
                }
            }

            // Create default Manager - FIXED
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
                    Console.WriteLine("Manager user created successfully!");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(managerUser, "Manager"))
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            Console.WriteLine("Seed data completed!");
        }
    }
}
