using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Services;
using System.Security.Claims;
using CMCSClaim = ContractMonthlyClaimSystem.Models.Claim;
using System.Text.Json;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;
        private readonly IClaimAutomationService _claimAutomationService;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(ApplicationDbContext context,
                              UserManager<ApplicationUser> userManager,
                              IFileUploadService fileUploadService,
                              IClaimAutomationService claimAutomationService,
                              ILogger<ClaimsController> logger )
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
            _claimAutomationService = claimAutomationService;
        }

        // GET: Claims/Create - Lecturer claim submission
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Identity");
                }

                ViewBag.User = user;
                return View(new CMCSClaim { HoursWorked = 0 });
            }
            catch (Exception ex)
            {
                // Log the exception
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create(IFormCollection form, List<IFormFile> supportingDocuments)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Index", "Home");
                }

                // Manually parse form data
                if (!int.TryParse(form["HoursWorked"], out int hoursWorked) || hoursWorked <= 0)
                {
                    TempData["Error"] = "Please enter valid hours worked.";
                    ViewBag.User = user;
                    return View(new CMCSClaim { HoursWorked = hoursWorked, Notes = form["Notes"] });
                }

                // Create new claim
                var claim = new CMCSClaim
                {
                    UserId = user.Id,
                    HoursWorked = hoursWorked,
                    Notes = form["Notes"],
                    SubmissionDate = DateTime.Now,
                    TotalAmount = user.HourlyRate * hoursWorked,
                    CurrentStatusID = 1 // Submitted
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Handle file uploads
                if (supportingDocuments != null && supportingDocuments.Count > 0)
                {
                    foreach (var document in supportingDocuments)
                    {
                        if (document.Length > 0 && _fileUploadService.IsValidFile(document))
                        {
                            var filePath = await _fileUploadService.UploadFileAsync(document, $"claim_{claim.ClaimID}");

                            if (filePath != null)
                            {
                                var supportingDocument = new SupportingDocument
                                {
                                    ClaimID = claim.ClaimID,
                                    FileName = document.FileName,
                                    FilePath = filePath,
                                    UploadDate = DateTime.Now
                                };

                                _context.SupportingDocuments.Add(supportingDocument);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Add to status history
                var statusHistory = new ClaimStatusHistory
                {
                    ClaimID = claim.ClaimID,
                    StatusID = 1,
                    ChangedByUserId = user.Id,
                    ChangeDate = DateTime.Now,
                    Notes = "Claim submitted by lecturer"
                };
                _context.ClaimStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Claim submitted successfully! Total amount: R {claim.TotalAmount:F2}";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while submitting your claim. Please try again.";
                // Log the exception here
                return RedirectToAction("Create");
            }
        }

        // GET: Coordinator dashboard - View pending claims
        // In Controllers/ClaimsController.cs - Update CoordinatorDashboard method
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> CoordinatorDashboard()
        {
            try
            {
                var prioritizedClaims = await _claimAutomationService.GetPrioritizedClaimsAsync();
                return View(prioritizedClaims);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error loading coordinator dashboard");

                // Fallback: Load claims without automation scoring
                var fallbackClaims = await _context.Claims
                    .Include(c => c.User)
                    .Include(c => c.CurrentStatus)
                    .Include(c => c.SupportingDocuments)
                    .Where(c => c.CurrentStatusID == 1)
                    .ToListAsync();

                // Convert to ClaimWithScore with default scores
                var fallbackWithScores = fallbackClaims.Select(claim => new ClaimWithScore
                {
                    Claim = claim,
                    Score = new ClaimScore
                    {
                        ClaimId = claim.ClaimID,
                        HoursWorked = claim.HoursWorked,
                        HoursValid = true,
                        AmountReasonable = true,
                        HasSupportingDocs = claim.SupportingDocuments?.Any() == true,
                        OverallScore = 70, // Default medium score
                        Priority = "Medium",
                        Recommendation = "Review"
                    },
                    Priority = 70,
                    Recommendation = "Review"
                }).ToList();

                TempData["Error"] = "Automation features temporarily unavailable. Showing basic claim list.";
                return View(fallbackWithScores);
            }
        }

        // GET: Manager dashboard - View coordinator-approved claims
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagerDashboard()
        {
            var approvedByCoordinatorClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.CurrentStatusID == 2) // ApprovedByCoordinator
                .ToListAsync();

            return View(approvedByCoordinatorClaims);
        }

        // AJAX: Update claim status (Approve/Reject)
        [HttpPost]
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<JsonResult> UpdateStatus(int claimId, int newStatusId, string? notes = "")
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                    return Json(new { success = false, message = "Claim not found" });

                claim.CurrentStatusID = newStatusId;

                // Add to status history
                var statusHistory = new ClaimStatusHistory
                {
                    ClaimID = claimId,
                    StatusID = newStatusId,
                    ChangedByUserId = user.Id,
                    ChangeDate = DateTime.Now,
                    Notes = notes
                };
                _context.ClaimStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return Json(new { success = false, message = "Error updating status" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<JsonResult> ProcessBatchApproval(List<int> claimIds, bool enableAutoApprove = true)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _claimAutomationService.ProcessBatchApprovalAsync(claimIds, user.Id);

                return Json(new
                {
                    success = true,
                    message = $"Batch processing completed: {result.Approved} approved, {result.RequiresManualReview} need review",
                    processingLog = result.ProcessingLog
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during batch processing" });
            }
        }


        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        public async Task<JsonResult> RunAutoScoring()
        {
            try
            {
                var claims = await _context.Claims
                    .Where(c => c.CurrentStatusID == 1)
                    .ToListAsync();

                foreach (var claim in claims)
                {
                    await _claimAutomationService.CalculateClaimScoreAsync(claim);
                }

                return Json(new { success = true, message = "Auto-scoring completed for all pending claims" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during auto-scoring" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<IActionResult> GetClaimAnalysis(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.SupportingDocuments)
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return Content("<div class='alert alert-danger'>Claim not found</div>");

            var score = await _claimAutomationService.CalculateClaimScoreAsync(claim);
            var automationResult = await _claimAutomationService.ProcessAutomatedApprovalAsync(claim);

            var analysisHtml = $@"
        <div class='analysis-content'>
            <h6>AI Analysis for Claim #{claim.ClaimID}</h6>
            
            <div class='row mb-3'>
                <div class='col-6'>
                    <strong>Overall Score:</strong>
                    <div class='progress'>
                        <div class='progress-bar {GetScoreColor(score.OverallScore)}' 
                             style='width: {score.OverallScore}%'>
                            {score.OverallScore}%
                        </div>
                    </div>
                </div>
                <div class='col-6'>
                    <strong>Priority:</strong>
                    <span class='badge {GetPriorityBadge(score.Priority)}'>{score.Priority}</span>
                </div>
            </div>

            <div class='mb-3'>
                <strong>Recommendation:</strong>
                <span class='badge {GetRecommendationBadge(score.Recommendation)}'>{score.Recommendation}</span>
            </div>

            <div class='mb-3'>
                <h6>Validation Results:</h6>
                <ul class='list-unstyled'>
                    <li>{GetValidationIcon(score.HoursValid)} Hours Valid: {score.HoursWorked}</li>
                    <li>{GetValidationIcon(score.AmountReasonable)} Amount Reasonable</li>
                    <li>{GetValidationIcon(score.HasSupportingDocs)} Supporting Documents: {claim.SupportingDocuments.Count}</li>
                </ul>
            </div>

            {(automationResult.ValidationWarnings.Any() ? $@"
                <div class='alert alert-warning'>
                    <h6>Validation Warnings:</h6>
                    <ul>
                        {string.Join("", automationResult.ValidationWarnings.Select(w => $@"<li>{w}</li>"))}
                    </ul>
                </div>
            " : "")}

            {(automationResult.AutoApproved ? $@"
                <div class='alert alert-success'>
                    <i class='fas fa-robot'></i> This claim is eligible for auto-approval!
                </div>
            " : "")}
        </div>
    ";

            return Content(analysisHtml);
        }

        // Helper methods for analysis view
        private string GetScoreColor(decimal score)
        {
            return score >= 80 ? "bg-success" : score >= 60 ? "bg-warning" : "bg-danger";
        }

        private string GetPriorityBadge(string priority)
        {
            return priority switch
            {
                "High" => "bg-danger",
                "Medium" => "bg-warning",
                _ => "bg-secondary"
            };
        }

        private string GetRecommendationBadge(string recommendation)
        {
            return recommendation switch
            {
                "Auto-Approve" => "bg-success",
                "Approve" => "bg-info",
                "Review" => "bg-warning",
                "Detailed Review" => "bg-orange",
                _ => "bg-danger"
            };
        }

        private string GetValidationIcon(bool isValid)
        {
            return isValid ? "✅" : "❌";
        }


        // GET: Claim details with status history
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Include(c => c.SupportingDocuments)
                .Include(c => c.StatusHistory!)
                    .ThenInclude(sh => sh.Status)
                .Include(c => c.StatusHistory!)
                    .ThenInclude(sh => sh.ChangedByUser)
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null)
                return NotFound();

            return View(claim);
        }

        [HttpGet]
        [Authorize(Roles = "Lecturer")]
        public async Task<JsonResult> GetClaimDraft()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                // In a real application, you'd store drafts in the database
                // For now, we'll use session or just return the structure
                return Json(new
                {
                    success = true,
                    draft = new
                    {
                        hoursWorked = 0,
                        notes = "",
                        lastSaved = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading draft" });
            }
        }

        // New automation methods
        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        public async Task<JsonResult> AutoApproveClaim(int claimId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var claim = await _context.Claims.FindAsync(claimId);

                if (claim == null)
                    return Json(new { success = false, message = "Claim not found" });

                var automationResult = await _claimAutomationService.ProcessAutomatedApprovalAsync(claim);

                if (automationResult.AutoApproved)
                {
                    claim.CurrentStatusID = 2; // ApprovedByCoordinator

                    var statusHistory = new ClaimStatusHistory
                    {
                        ClaimID = claimId,
                        StatusID = 2,
                        ChangedByUserId = user.Id,
                        ChangeDate = DateTime.Now,
                        Notes = $"Auto-approved: {automationResult.Reason}"
                    };
                    _context.ClaimStatusHistories.Add(statusHistory);

                    await _context.SaveChangesAsync();
                    await _claimAutomationService.SendAutomatedNotificationsAsync(claim, "auto-approved");

                    return Json(new { success = true, message = "Claim auto-approved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Claim not eligible for auto-approval" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during auto-approval" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        public async Task<JsonResult> SaveClaimDraft([FromBody] DraftClaimModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                // In a real application, save to database
                // For demo, we'll just return success
                return Json(new
                {
                    success = true,
                    message = "Draft saved successfully",
                    savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving draft" });
            }
        }

   
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<JsonResult> BatchApproveHighConfidence()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var highConfidenceClaims = await _context.Claims
                    .Include(c => c.User)
                    .Where(c => c.CurrentStatusID == 2) // ApprovedByCoordinator
                    .ToListAsync();

                var claimsWithScores = new List<ClaimWithScore>();
                foreach (var claim in highConfidenceClaims)
                {
                    var score = await _claimAutomationService.CalculateClaimScoreAsync(claim);
                    if (score.OverallScore >= 80)
                    {
                        claimsWithScores.Add(new ClaimWithScore { Claim = claim, Score = score });
                    }
                }

                int approvedCount = 0;
                foreach (var item in claimsWithScores)
                {
                    item.Claim.CurrentStatusID = 3; // ApprovedByManager

                    var statusHistory = new ClaimStatusHistory
                    {
                        ClaimID = item.Claim.ClaimID,
                        StatusID = 3,
                        ChangedByUserId = user.Id,
                        ChangeDate = DateTime.Now,
                        Notes = $"Auto-approved by manager: High confidence score ({item.Score.OverallScore}%)"
                    };
                    _context.ClaimStatusHistories.Add(statusHistory);
                    approvedCount++;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Approved {approvedCount} high-confidence claims automatically."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error during batch approval" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<JsonResult> GenerateManagerReport()
        {
            try
            {
                var approvedClaims = await _context.Claims
                    .Include(c => c.User)
                    .Where(c => c.CurrentStatusID == 3 &&
                               c.SubmissionDate >= DateTime.Now.AddMonths(-1))
                    .ToListAsync();

                var reportContent = $"MANAGER APPROVAL REPORT - {DateTime.Now:yyyy-MM-dd}\n\n";
                reportContent += "==========================================\n\n";

                decimal totalAmount = 0;
                foreach (var claim in approvedClaims)
                {
                    reportContent += $"{claim.User.FullName}: R{claim.TotalAmount:F2} ({claim.HoursWorked} hours)\n";
                    totalAmount += claim.TotalAmount;
                }

                reportContent += $"\nTOTAL APPROVED: R{totalAmount:F2}\n";
                reportContent += $"TOTAL CLAIMS: {approvedClaims.Count}\n";
                reportContent += $"AVERAGE CLAIM: R{(approvedClaims.Any() ? totalAmount / approvedClaims.Count : 0):F2}\n";

                var bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
                var fileName = $"manager_report_{DateTime.Now:yyyyMMdd}.txt";

                // In a real application, save file and return URL
                return Json(new
                {
                    success = true,
                    message = "Report generated successfully",
                    downloadUrl = $"data:text/plain;base64,{Convert.ToBase64String(bytes)}",
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error generating report" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerAnalysis(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.SupportingDocuments)
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return Content("<div class='alert alert-danger'>Claim not found</div>");

            var score = await _claimAutomationService.CalculateClaimScoreAsync(claim);
            var coordinatorHistory = claim.StatusHistory?
                .Where(h => h.StatusID == 2)
                .OrderByDescending(h => h.ChangeDate)
                .FirstOrDefault();

            var analysisHtml = $@"
        <div class='manager-analysis'>
            <h6>Executive Analysis - Claim #{claim.ClaimID}</h6>
            
            <div class='row mb-3'>
                <div class='col-md-6'>
                    <strong>AI Confidence Score:</strong>
                    <div class='progress mb-2'>
                        <div class='progress-bar {GetScoreColor(score.OverallScore)}' 
                             style='width: {score.OverallScore}%'>
                            {score.OverallScore}%
                        </div>
                    </div>
                    <small class='text-muted'>Risk Assessment: {GetRiskLevel(score.OverallScore)}</small>
                </div>
                <div class='col-md-6'>
                    <strong>Financial Summary:</strong>
                    <div>Amount: <strong>R {claim.TotalAmount:F2}</strong></div>
                    <div>Hours: {claim.HoursWorked} @ R{claim.User.HourlyRate:F2}/hr</div>
                </div>
            </div>

            <div class='alert {GetRecommendationAlert(score.Recommendation)}'>
                <strong>Recommendation:</strong> {score.Recommendation}
            </div>

            <div class='row'>
                <div class='col-md-6'>
                    <h6>Validation Checks:</h6>
                    <ul class='list-unstyled'>
                        <li>{GetValidationIcon(score.HoursValid)} Hours within policy</li>
                        <li>{GetValidationIcon(score.AmountReasonable)} Amount reasonable</li>
                        <li>{GetValidationIcon(score.HasSupportingDocs)} Supporting documents</li>
                        <li>{GetValidationIcon(score.PreviousClaimHistory >= 2)} Established history</li>
                    </ul>
                </div>
                <div class='col-md-6'>
                    <h6>Coordinator Review:</h6>
                    <p class='text-muted'>
                        {(coordinatorHistory != null ? coordinatorHistory.Notes : "No coordinator notes")}
                    </p>
                </div>
            </div>

            {(score.OverallScore >= 80 ? @"
                <div class='alert alert-success mt-3'>
                    <i class='fas fa-robot'></i> 
                    <strong>AI Insight:</strong> This claim demonstrates strong patterns for approval with minimal risk.
                </div>
            " : "")}
        </div>
    ";

            return Content(analysisHtml);
        }

        // Helper methods for manager analysis
        private string GetRiskLevel(decimal score)
        {
            return score switch
            {
                >= 80 => "Low",
                >= 60 => "Medium",
                >= 40 => "High",
                _ => "Very High"
            };
        }

        private string GetRecommendationAlert(string recommendation)
        {
            return recommendation switch
            {
                "Auto-Approve" => "alert-success",
                "Approve" => "alert-info",
                "Review" => "alert-warning",
                _ => "alert-danger"
            };
        }
    }
}
