using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity;



namespace ContractMonthlyClaimSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ FIX: Configure decimal precision for HourlyRate
            builder.Entity<ApplicationUser>()
                .Property(u => u.HourlyRate)
                .HasPrecision(18, 2); // 18 digits total, 2 after decimal

            // ✅ FIX: Configure decimal precision for TotalAmount in Claim
            builder.Entity<Claim>()
                .Property(c => c.TotalAmount)
                .HasPrecision(18, 2);

            // Configure relationships
            builder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Claim>()
                .HasOne(c => c.CurrentStatus)
                .WithMany()
                .HasForeignKey(c => c.CurrentStatusID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupportingDocument>()
                .HasOne(sd => sd.Claim)
                .WithMany(c => c.SupportingDocuments)
                .HasForeignKey(sd => sd.ClaimID);

            builder.Entity<ClaimStatusHistory>()
                .HasOne(csh => csh.Claim)
                .WithMany(c => c.StatusHistory)
                .HasForeignKey(csh => csh.ClaimID);

            builder.Entity<ClaimStatusHistory>()
                .HasOne(csh => csh.Status)
                .WithMany()
                .HasForeignKey(csh => csh.StatusID);

            builder.Entity<ClaimStatusHistory>()
                .HasOne(csh => csh.ChangedByUser)
                .WithMany()
                .HasForeignKey(csh => csh.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial status data
            builder.Entity<Status>().HasData(
                new Status { StatusID = 1, StatusName = "Submitted", Description = "Claim submitted by lecturer" },
                new Status { StatusID = 2, StatusName = "ApprovedByCoordinator", Description = "Approved by programme coordinator" },
                new Status { StatusID = 3, StatusName = "ApprovedByManager", Description = "Approved by academic manager" },
                new Status { StatusID = 4, StatusName = "Rejected", Description = "Claim rejected" },
                new Status { StatusID = 5, StatusName = "Paid", Description = "Claim has been paid" }
            );
        }
    }
}