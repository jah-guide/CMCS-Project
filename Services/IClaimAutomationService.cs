// File: Services/IClaimAutomationService.cs
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IClaimAutomationService
    {
        Task<ClaimScore> CalculateClaimScoreAsync(Claim claim);
        Task<List<ClaimWithScore>> GetPrioritizedClaimsAsync();
        Task<AutomatedWorkflowResult> ProcessAutomatedApprovalAsync(Claim claim);
        Task<BatchApprovalResult> ProcessBatchApprovalAsync(List<int> claimIds, string approvedBy);
        Task SendAutomatedNotificationsAsync(Claim claim, string action);
    }

    public class ClaimWithScore
    {
        public Claim Claim { get; set; } = new();
        public ClaimScore Score { get; set; } = new();
        public int Priority { get; set; }
        public string Recommendation { get; set; } = "Review";
    }

    public class AutomatedWorkflowResult
    {
        public bool AutoApproved { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> ValidationWarnings { get; set; } = new();
    }

    public class BatchApprovalResult
    {
        public int TotalProcessed { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int RequiresManualReview { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> ProcessingLog { get; set; } = new();
    }
}