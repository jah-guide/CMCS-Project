using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Services;
using System.Security.Claims;
using CMCSClaim = ContractMonthlyClaimSystem.Models.Claim;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public ClaimsController(ApplicationDbContext context,
                              UserManager<ApplicationUser> userManager,
                              IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
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
        // POST: Claims/Create - SIMPLIFIED VERSION
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
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> CoordinatorDashboard()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.CurrentStatusID == 1) // Submitted
                .ToListAsync();

            return View(pendingClaims);
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
    }
}
