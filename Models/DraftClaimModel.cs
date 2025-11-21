namespace ContractMonthlyClaimSystem.Models
{
    public class DraftClaimModel
    {
        public int HoursWorked { get; set; }
        public string? Notes { get; set; }
        public List<string>? FileNames { get; set; }
    }
}
