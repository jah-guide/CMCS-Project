using System.ComponentModel.DataAnnotations;


namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        public int SupportingDocumentID { get; set; }

        [Required]
        public int ClaimID { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Claim? Claim { get; set; }
    }
}