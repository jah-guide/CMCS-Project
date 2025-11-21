// File: Services/ClaimAutomationService.cs
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimAutomationService : IClaimAutomationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClaimAutomationService> _logger;

        public ClaimAutomationService(ApplicationDbContext context, ILogger<ClaimAutomationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClaimScore> CalculateClaimScoreAsync(Claim claim)
        {
            var userClaims = await _context.Claims
                .Where(c => c.UserId == claim.UserId && c.ClaimID != claim.ClaimID)
                .ToListAsync();

            var score = new ClaimScore
            {
                ClaimId = claim.ClaimID,
                HoursWorked = claim.HoursWorked, // ADD THIS LINE
                HoursValid = claim.HoursWorked <= 200 && claim.HoursWorked >= 1,
                AmountReasonable = await IsAmountReasonableAsync(claim),
                HasSupportingDocs = claim.SupportingDocuments?.Any() == true,
                PreviousClaimHistory = userClaims.Count(c => c.CurrentStatusID != 4), // Not rejected
                SubmissionPattern = await AnalyzeSubmissionPatternAsync(claim.UserId, claim.SubmissionDate),
                DocumentQuality = await AnalyzeDocumentQualityAsync(claim.SupportingDocuments)
            };

            score.OverallScore = CalculateOverallScore(score);
            score.Priority = DeterminePriority(score);
            score.Recommendation = GenerateRecommendation(score);

            return score;
        }

        public async Task<List<ClaimWithScore>> GetPrioritizedClaimsAsync()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.CurrentStatus)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.CurrentStatusID == 1) // Submitted
                .ToListAsync();

            var claimsWithScores = new List<ClaimWithScore>();

            foreach (var claim in pendingClaims)
            {
                var score = await CalculateClaimScoreAsync(claim);
                var priority = CalculatePriorityScore(score);

                claimsWithScores.Add(new ClaimWithScore
                {
                    Claim = claim,
                    Score = score,
                    Priority = priority,
                    Recommendation = score.Recommendation
                });
            }

            return claimsWithScores
                .OrderByDescending(c => c.Priority)
                .ThenByDescending(c => c.Score.OverallScore)
                .ToList();
        }

        public async Task<AutomatedWorkflowResult> ProcessAutomatedApprovalAsync(Claim claim)
        {
            var result = new AutomatedWorkflowResult();
            var score = await CalculateClaimScoreAsync(claim);

            // Auto-approval rules
            if (score.OverallScore >= 90 && score.HoursValid && score.AmountReasonable && score.HasSupportingDocs)
            {
                result.AutoApproved = true;
                result.Reason = "High-confidence claim with excellent score and complete documentation";
            }
            else if (score.OverallScore >= 80 && score.HoursValid && score.AmountReasonable)
            {
                result.AutoApproved = true;
                result.Reason = "Confident approval based on strong claim history and validation";
            }
            else if (score.OverallScore <= 40)
            {
                result.ValidationWarnings.Add("Low confidence score - requires manual review");
            }

            // Add validation warnings
            if (!score.HoursValid)
                result.ValidationWarnings.Add("Hours worked outside valid range");
            if (!score.AmountReasonable)
                result.ValidationWarnings.Add("Claim amount appears unreasonable");
            if (!score.HasSupportingDocs)
                result.ValidationWarnings.Add("No supporting documents provided");

            return result;
        }

        public async Task<BatchApprovalResult> ProcessBatchApprovalAsync(List<int> claimIds, string approvedBy)
        {
            var result = new BatchApprovalResult();
            var claims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.SupportingDocuments)
                .Where(c => claimIds.Contains(c.ClaimID))
                .ToListAsync();

            foreach (var claim in claims)
            {
                var automationResult = await ProcessAutomatedApprovalAsync(claim);

                if (automationResult.AutoApproved)
                {
                    // Auto-approve high-confidence claims
                    claim.CurrentStatusID = 2; // ApprovedByCoordinator

                    var statusHistory = new ClaimStatusHistory
                    {
                        ClaimID = claim.ClaimID,
                        StatusID = 2,
                        ChangedByUserId = approvedBy,
                        ChangeDate = DateTime.Now,
                        Notes = $"Auto-approved: {automationResult.Reason}"
                    };
                    _context.ClaimStatusHistories.Add(statusHistory);

                    result.Approved++;
                    result.ProcessingLog.Add($"✅ Auto-approved Claim {claim.ClaimID}: {automationResult.Reason}");
                }
                else if (automationResult.ValidationWarnings.Any())
                {
                    // Flag for manual review
                    result.RequiresManualReview++;
                    result.ProcessingLog.Add($"⚠️ Manual review needed for Claim {claim.ClaimID}: {string.Join(", ", automationResult.ValidationWarnings)}");
                }
                else
                {
                    // Standard approval
                    claim.CurrentStatusID = 2;

                    var statusHistory = new ClaimStatusHistory
                    {
                        ClaimID = claim.ClaimID,
                        StatusID = 2,
                        ChangedByUserId = approvedBy,
                        ChangeDate = DateTime.Now,
                        Notes = "Approved in batch processing"
                    };
                    _context.ClaimStatusHistories.Add(statusHistory);

                    result.Approved++;
                    result.ProcessingLog.Add($"✅ Approved Claim {claim.ClaimID}");
                }

                result.TotalAmount += claim.TotalAmount;
            }

            await _context.SaveChangesAsync();
            result.TotalProcessed = claims.Count;

            return result;
        }

        public async Task SendAutomatedNotificationsAsync(Claim claim, string action)
        {
            // In a real application, this would send emails or notifications
            _logger.LogInformation($"Notification: Claim {claim.ClaimID} {action} for {claim.User.FullName}");

            // Simulate notification delay
            await Task.Delay(100);
        }

        // Private helper methods
        // In Services/ClaimAutomationService.cs - Fix the IsAmountReasonableAsync method
        private async Task<bool> IsAmountReasonableAsync(Claim claim)
        {
            var userClaims = await _context.Claims
                .Where(c => c.UserId == claim.UserId && c.ClaimID != claim.ClaimID)
                .ToListAsync();

            if (!userClaims.Any()) return true; // No history, assume reasonable

            var averageAmount = userClaims.Average(c => c.TotalAmount);

            // FIX: Check if averageAmount is zero to avoid division by zero
            if (averageAmount == 0) return true;

            var deviation = Math.Abs(claim.TotalAmount - averageAmount) / averageAmount;

            return deviation <= 0.5m; // Use decimal literal instead of double
        }

        private async Task<int> AnalyzeSubmissionPatternAsync(string userId, DateTime submissionDate)
        {
            var userClaims = await _context.Claims
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();

            if (userClaims.Count < 2) return 5; // Neutral score for new users

            var lastSubmission = userClaims[^2].SubmissionDate;
            var daysBetween = (submissionDate - lastSubmission).Days;

            return daysBetween switch
            {
                < 15 => 1,  // Too frequent
                < 30 => 3,  // Regular
                _ => 5      // Good spacing
            };
        }

        // In Services/ClaimAutomationService.cs - REPLACE the AnalyzeDocumentQualityAsync method
        private async Task<int> AnalyzeDocumentQualityAsync(ICollection<SupportingDocument> documents)
        {
            if (documents == null || !documents.Any())
                return 1; // Poor quality if no documents

            try
            {
                int qualityScore = 3; // Default average score

                // Document count analysis
                if (documents.Count >= 3)
                    qualityScore += 2; // Bonus for multiple documents
                else if (documents.Count == 2)
                    qualityScore += 1; // Small bonus for two documents
                else if (documents.Count == 1)
                    qualityScore -= 1; // Penalty for single document

                // File type analysis
                var fileTypes = documents.Select(d => Path.GetExtension(d.FileName).ToLower()).ToList();
                var uniqueFileTypes = fileTypes.Distinct().Count();

                if (uniqueFileTypes >= 2)
                    qualityScore += 1; // Bonus for multiple file types

                // Check for important document types
                var hasPdf = fileTypes.Any(ext => ext == ".pdf");
                var hasSpreadsheet = fileTypes.Any(ext => ext == ".xlsx" || ext == ".xls");
                var hasImage = fileTypes.Any(ext => ext == ".jpg" || ext == ".jpeg" || ext == ".png");

                if (hasPdf) qualityScore += 1; // PDFs are good for contracts/reports
                if (hasSpreadsheet) qualityScore += 1; // Spreadsheets are good for timesheets
                if (hasImage) qualityScore += 1; // Images are good for proof

                // File name quality analysis (basic)
                var hasDescriptiveNames = documents.Any(d =>
                    d.FileName.Length > 10 &&
                    !d.FileName.StartsWith("Screenshot") &&
                    !d.FileName.StartsWith("image"));

                if (hasDescriptiveNames)
                    qualityScore += 1;

                return Math.Clamp(qualityScore, 1, 10); // Ensure score is between 1-10
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error analyzing document quality: {ex.Message}");
                return 3; // Default average score on error
            }
        }

        private decimal CalculateOverallScore(ClaimScore score)
        {
            decimal totalScore = 0;

            if (score.HoursValid) totalScore += 20;
            if (score.AmountReasonable) totalScore += 20;
            if (score.HasSupportingDocs) totalScore += 20;
            totalScore += Math.Min(score.PreviousClaimHistory * 3, 15); // History
            totalScore += Math.Min(score.SubmissionPattern * 2, 10);    // Pattern
            totalScore += Math.Min(score.DocumentQuality * 2, 15);      // Document quality (now out of 10)

            return totalScore;
        }

        private string DeterminePriority(ClaimScore score)
        {
            return score.OverallScore switch
            {
                >= 80 => "High",
                >= 60 => "Medium",
                _ => "Low"
            };
        }

        private int CalculatePriorityScore(ClaimScore score)
        {
            var baseScore = score.OverallScore;

            // Boost priority for high amounts or urgent cases
            if (score.Priority == "High") baseScore += 20;
            if (!score.HasSupportingDocs) baseScore -= 15;
            if (!score.AmountReasonable) baseScore -= 10;

            return (int)Math.Max(0, baseScore);
        }

        private string GenerateRecommendation(ClaimScore score)
        {
            if (score.OverallScore >= 85) return "Auto-Approve";
            if (score.OverallScore >= 70) return "Approve";
            if (score.OverallScore >= 50) return "Review";
            if (score.OverallScore >= 30) return "Detailed Review";
            return "Investigate";
        }
    }
}