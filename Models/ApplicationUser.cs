using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;


        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; } = 0;

        // Navigation properties - make them nullable
        public virtual ICollection<Claim>? Claims { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}