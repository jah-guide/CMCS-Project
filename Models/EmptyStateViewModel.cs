namespace ContractMonthlyClaimSystem.Models
{
    public class EmptyStateViewModel
    {
        public string Title { get; set; } = "Nothing here yet";
        public string Message { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fa-inbox";
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
    }
}
