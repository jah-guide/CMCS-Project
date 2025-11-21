using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 200)]
        public int HoursWorked { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public int CurrentStatusID { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties - make them nullable
        public virtual ApplicationUser? User { get; set; }
        public virtual Status? CurrentStatus { get; set; }
        public virtual ICollection<SupportingDocument>? SupportingDocuments { get; set; }
        public virtual ICollection<ClaimStatusHistory>? StatusHistory { get; set; }
    }
}
