using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimStatusHistory
    {
        public int ClaimStatusHistoryID { get; set; }

        [Required]
        public int ClaimID { get; set; }

        [Required]
        public int StatusID { get; set; }

        [Required]
        public string ChangedByUserId { get; set; } = string.Empty;

        [Required]
        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties - make them nullable
        public virtual Claim? Claim { get; set; }
        public virtual Status? Status { get; set; }
        public virtual ApplicationUser? ChangedByUser { get; set; }
    }
}
