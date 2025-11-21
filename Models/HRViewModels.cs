// File: Models/HRViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    // HR Dashboard main view model
    public class HRDashboardViewModel
    {
        public List<Claim> PendingPaymentClaims { get; set; } = new();
        public MonthlySummary MonthlySummary { get; set; } = new();
        public PaymentBatchViewModel CurrentPaymentBatch { get; set; } = new();
        public List<ApplicationUser> Lecturers { get; set; } = new();
        public List<ClaimScore> ClaimScores { get; set; } = new();
    }

    // Monthly summary for dashboard
    public class MonthlySummary
    {
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public int ApprovedClaims { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int PaidClaims { get; set; }
        public decimal PaidAmount { get; set; }
        public int PendingApprovalClaims { get; set; }
        public int RejectedClaims { get; set; }
    }

    // Payment batch processing
    public class PaymentBatchViewModel
    {
        public int BatchId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ClaimCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public List<Claim> Claims { get; set; } = new();
    }

    // Automated claim scoring for prioritization
    public class ClaimScore
    {
        public int ClaimId { get; set; }
        public bool HoursValid { get; set; }
        public bool AmountReasonable { get; set; }
        public bool HasSupportingDocs { get; set; }
        public int PreviousClaimHistory { get; set; }
        public int SubmissionPattern { get; set; }
        public int DocumentQuality { get; set; }
        public decimal OverallScore { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Recommendation { get; set; } = "Review";
        public int HoursWorked { get; set; }
    }

    // Invoice generation model
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public int HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string ClaimPeriod { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = new();
    }

    // Lecturer management view model
    public class LecturerManagementViewModel
    {
        public ApplicationUser Lecturer { get; set; } = new();
        public List<Claim> RecentClaims { get; set; } = new();
        public decimal TotalPaidThisMonth { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public int SuccessfulClaims { get; set; }
        public int RejectedClaims { get; set; }
    }

    // Report generation parameters
    public class ReportParameters
    {
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public string ReportType { get; set; } = "Payment";
        public string Format { get; set; } = "PDF";
        public bool IncludeDetails { get; set; } = true;
    }

    // Automated workflow rules
    public class AutomatedWorkflow
    {
        public int WorkflowId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public string TriggerEvent { get; set; } = "ClaimSubmitted";
        public List<WorkflowRule> Rules { get; set; } = new();
        public string ActionType { get; set; } = "AutoApprove";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class WorkflowRule
    {
        public int RuleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Property { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Operator { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        public int Sequence { get; set; }
    }

    // Bulk operations model
    public class BulkOperationViewModel
    {
        public List<int> SelectedClaimIds { get; set; } = new();
        public string OperationType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; } = DateTime.Now;
    }

    // Dashboard statistics
    public class DashboardStats
    {
        public int TotalLecturers { get; set; }
        public int ActiveClaims { get; set; }
        public decimal MonthlyBudget { get; set; }
        public decimal UtilizedBudget { get; set; }
        public decimal BudgetUtilizationPercent => MonthlyBudget > 0 ? (UtilizedBudget / MonthlyBudget) * 100 : 0;
        public int ClaimsProcessedToday { get; set; }
        public decimal AverageProcessingTimeHours { get; set; }
    }
}
