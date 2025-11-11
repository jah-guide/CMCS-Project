using System.ComponentModel.DataAnnotations;


namespace ContractMonthlyClaimSystem.Models
{
    public class Status
    {
        public int StatusID { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}