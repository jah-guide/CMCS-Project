// File: Services/ReportService.cs
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ReportService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<MonthlySummary> GenerateMonthlySummaryAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var claims = await _context.Claims
                .Include(c => c.CurrentStatus)
                .Where(c => c.SubmissionDate >= startDate && c.SubmissionDate <= endDate)
                .ToListAsync();

            return new MonthlySummary
            {
                TotalClaims = claims.Count,
                TotalAmount = claims.Sum(c => c.TotalAmount),
                ApprovedClaims = claims.Count(c => c.CurrentStatusID == 3), // ApprovedByManager
                ApprovedAmount = claims.Where(c => c.CurrentStatusID == 3).Sum(c => c.TotalAmount),
                PaidClaims = claims.Count(c => c.CurrentStatusID == 5), // Paid
                PaidAmount = claims.Where(c => c.CurrentStatusID == 5).Sum(c => c.TotalAmount)
            };
        }

        public async Task<PaymentBatchViewModel> GeneratePaymentBatchAsync()
        {
            var approvedClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Where(c => c.CurrentStatusID == 3) // ApprovedByManager
                .ToListAsync();

            return new PaymentBatchViewModel
            {
                BatchId = new Random().Next(1000, 9999),
                CreatedDate = DateTime.Now,
                ClaimCount = approvedClaims.Count,
                TotalAmount = approvedClaims.Sum(c => c.TotalAmount),
                Claims = approvedClaims
            };
        }

        public async Task<byte[]> GenerateInvoicesAsync(List<int> claimIds)
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.SupportingDocuments)
                .Where(c => claimIds.Contains(c.ClaimID))
                .ToListAsync();

            // Simple PDF generation (in real scenario, use a PDF library)
            var invoiceContent = $"INVOICE BATCH - {DateTime.Now:yyyy-MM-dd}\n\n";
            invoiceContent += "==========================================\n\n";

            foreach (var claim in claims)
            {
                invoiceContent += $"Claim ID: {claim.ClaimID}\n";
                invoiceContent += $"Lecturer: {claim.User.FullName}\n";
                invoiceContent += $"Hours: {claim.HoursWorked} @ R{claim.User.HourlyRate:F2}/hr\n";
                invoiceContent += $"Total: R{claim.TotalAmount:F2}\n";
                invoiceContent += $"Submission Date: {claim.SubmissionDate:yyyy-MM-dd}\n";
                invoiceContent += "------------------------------------------\n\n";
            }

            invoiceContent += $"TOTAL BATCH AMOUNT: R{claims.Sum(c => c.TotalAmount):F2}\n";
            invoiceContent += $"TOTAL CLAIMS: {claims.Count}\n";

            return System.Text.Encoding.UTF8.GetBytes(invoiceContent);
        }

        public async Task<byte[]> GeneratePaymentReportAsync(DateTime startDate, DateTime endDate)
        {
            var paidClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.CurrentStatusID == 5 && c.SubmissionDate >= startDate && c.SubmissionDate <= endDate)
                .ToListAsync();

            var reportContent = $"PAYMENT REPORT: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}\n\n";
            reportContent += "==========================================\n\n";

            foreach (var claim in paidClaims)
            {
                reportContent += $"{claim.User.FullName}: R{claim.TotalAmount:F2} ({claim.HoursWorked} hours)\n";
            }

            reportContent += $"\nTOTAL PAID: R{paidClaims.Sum(c => c.TotalAmount):F2}\n";
            reportContent += $"TOTAL CLAIMS: {paidClaims.Count}\n";

            return System.Text.Encoding.UTF8.GetBytes(reportContent);
        }

        public async Task<ClaimScore> CalculateClaimScoreAsync(Claim claim)
        {
            var userClaims = await _context.Claims
                .Where(c => c.UserId == claim.UserId && c.ClaimID != claim.ClaimID)
                .ToListAsync();

            var score = new ClaimScore
            {
                ClaimId = claim.ClaimID,
                HoursValid = claim.HoursWorked <= 200 && claim.HoursWorked >= 1,
                AmountReasonable = claim.TotalAmount <= (claim.User.HourlyRate * 200),
                HasSupportingDocs = claim.SupportingDocuments?.Any() == true,
                PreviousClaimHistory = userClaims.Count(c => c.CurrentStatusID != 4) // Not rejected
            };

            // Calculate overall score
            score.OverallScore = CalculateOverallScore(score);
            score.Priority = DeterminePriority(score);

            return score;
        }

        private decimal CalculateOverallScore(ClaimScore score)
        {
            decimal totalScore = 0;
            if (score.HoursValid) totalScore += 25;
            if (score.AmountReasonable) totalScore += 25;
            if (score.HasSupportingDocs) totalScore += 25;
            totalScore += Math.Min(score.PreviousClaimHistory * 5, 25); // Max 25 for history

            return totalScore;
        }

        private string DeterminePriority(ClaimScore score)
        {
            return score.OverallScore >= 80 ? "High" :
                   score.OverallScore >= 60 ? "Medium" : "Low";
        }
    }
}