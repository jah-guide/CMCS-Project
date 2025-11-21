// File: Controllers/HRController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;

        public HRController(ApplicationDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var approvedClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Where(c => c.CurrentStatusID == 3) // ApprovedByManager
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            var lecturers = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lecturer")))
                .ToListAsync();

            // Calculate claim scores
            var claimScores = new List<ClaimScore>();
            foreach (var claim in approvedClaims)
            {
                var score = await _reportService.CalculateClaimScoreAsync(claim);
                claimScores.Add(score);
            }

            var viewModel = new HRDashboardViewModel
            {
                PendingPaymentClaims = approvedClaims,
                MonthlySummary = await _reportService.GenerateMonthlySummaryAsync(),
                CurrentPaymentBatch = await _reportService.GeneratePaymentBatchAsync(),
                Lecturers = lecturers,
                ClaimScores = claimScores
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayments(List<int> claimIds)
        {
            if (claimIds == null || !claimIds.Any())
            {
                TempData["Error"] = "No claims selected for payment processing.";
                return RedirectToAction("Dashboard");
            }

            var claims = await _context.Claims
                .Where(c => claimIds.Contains(c.ClaimID))
                .ToListAsync();

            foreach (var claim in claims)
            {
                claim.CurrentStatusID = 5; // Paid status

                // Add to status history
                var statusHistory = new ClaimStatusHistory
                {
                    ClaimID = claim.ClaimID,
                    StatusID = 5,
                    ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    ChangeDate = DateTime.Now,
                    Notes = "Processed for payment by HR"
                };
                _context.ClaimStatusHistories.Add(statusHistory);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully processed {claims.Count} claims for payment. Total amount: R{claims.Sum(c => c.TotalAmount):F2}";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoices(List<int> claimIds)
        {
            if (claimIds == null || !claimIds.Any())
            {
                TempData["Error"] = "No claims selected for invoice generation.";
                return RedirectToAction("Dashboard");
            }

            var invoices = await _reportService.GenerateInvoicesAsync(claimIds);
            return File(invoices, "application/pdf", $"invoices_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        public async Task<IActionResult> LecturerManagement()
        {
            var lecturers = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lecturer")))
                .OrderBy(u => u.LastName)
                .ToListAsync();

            return View(lecturers);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLecturerRate(string userId, decimal hourlyRate)
        {
            var lecturer = await _context.Users.FindAsync(userId);
            if (lecturer == null)
            {
                TempData["Error"] = "Lecturer not found.";
                return RedirectToAction("LecturerManagement");
            }

            lecturer.HourlyRate = hourlyRate;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Hourly rate updated to R{hourlyRate:F2} for {lecturer.FullName}";
            return RedirectToAction("LecturerManagement");
        }

        public async Task<IActionResult> Reports()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePaymentReport(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GeneratePaymentReportAsync(startDate, endDate);
            return File(report, "application/pdf", $"payment_report_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.pdf");
        }

        // Add to Controllers/HRController.cs
        [HttpPost]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GenerateComprehensiveReport(ReportParameters parameters)
        {
            try
            {
                var claims = await _context.Claims
                    .Include(c => c.User)
                    .Include(c => c.CurrentStatus)
                    .Where(c => c.SubmissionDate >= parameters.StartDate &&
                               c.SubmissionDate <= parameters.EndDate)
                    .ToListAsync();

                var reportData = new
                {
                    Period = $"{parameters.StartDate:yyyy-MM-dd} to {parameters.EndDate:yyyy-MM-dd}",
                    TotalClaims = claims.Count,
                    TotalAmount = claims.Sum(c => c.TotalAmount),
                    StatusBreakdown = claims.GroupBy(c => c.CurrentStatus.StatusName)
                                          .Select(g => new { Status = g.Key, Count = g.Count(), Amount = g.Sum(c => c.TotalAmount) }),
                    LecturerPerformance = claims.GroupBy(c => c.User.FullName)
                                              .Select(g => new { Lecturer = g.Key, Claims = g.Count(), TotalAmount = g.Sum(c => c.TotalAmount) })
                                              .OrderByDescending(x => x.TotalAmount),
                    AverageProcessingTime = await CalculateAverageProcessingTime(parameters.StartDate, parameters.EndDate)
                };

                // Generate comprehensive report
                var reportContent = GenerateReportContent(reportData, parameters);

                if (parameters.Format == "PDF")
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
                    return File(bytes, "application/pdf", $"comprehensive_report_{DateTime.Now:yyyyMMdd}.pdf");
                }
                else
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
                    return File(bytes, "text/csv", $"comprehensive_report_{DateTime.Now:yyyyMMdd}.csv");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error generating comprehensive report";
                return RedirectToAction("Reports");
            }
        }

        private async Task<decimal> CalculateAverageProcessingTime(DateTime startDate, DateTime endDate)
        {
            var completedClaims = await _context.Claims
                .Include(c => c.StatusHistory)
                .Where(c => c.SubmissionDate >= startDate && c.SubmissionDate <= endDate &&
                           (c.CurrentStatusID == 3 || c.CurrentStatusID == 4)) // Approved or Rejected
                .ToListAsync();

            if (!completedClaims.Any()) return 0;

            decimal totalHours = 0;
            foreach (var claim in completedClaims)
            {
                var submission = claim.SubmissionDate;
                var completion = claim.StatusHistory?
                    .Where(h => h.StatusID == 3 || h.StatusID == 4)
                    .OrderByDescending(h => h.ChangeDate)
                    .FirstOrDefault()?.ChangeDate ?? submission;

                totalHours += (decimal)(completion - submission).TotalHours;
            }

            return totalHours / completedClaims.Count;
        }

        private string GenerateReportContent(dynamic reportData, ReportParameters parameters)
        {
            // Comprehensive report generation
            var content = $"COMPREHENSIVE CLAIMS REPORT\n";
            content += $"Period: {reportData.Period}\n";
            content += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}\n";
            content += "=".PadRight(50, '=') + "\n\n";

            content += $"TOTAL CLAIMS: {reportData.TotalClaims}\n";
            content += $"TOTAL AMOUNT: R {reportData.TotalAmount:F2}\n";
            content += $"AVG PROCESSING TIME: {reportData.AverageProcessingTime:F1} hours\n\n";

            content += "STATUS BREAKDOWN:\n";
            foreach (var status in reportData.StatusBreakdown)
            {
                content += $"- {status.Status}: {status.Count} claims (R {status.Amount:F2})\n";
            }

            content += "\nLECTURER PERFORMANCE:\n";
            foreach (var lecturer in reportData.LecturerPerformance)
            {
                content += $"- {lecturer.Lecturer}: {lecturer.Claims} claims (R {lecturer.TotalAmount:F2})\n";
            }

            return content;
        }
    }
}
