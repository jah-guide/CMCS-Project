// File: Services/IReportService.cs
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IReportService
    {
        Task<MonthlySummary> GenerateMonthlySummaryAsync();
        Task<PaymentBatchViewModel> GeneratePaymentBatchAsync();
        Task<byte[]> GenerateInvoicesAsync(List<int> claimIds);
        Task<byte[]> GeneratePaymentReportAsync(DateTime startDate, DateTime endDate);
        Task<ClaimScore> CalculateClaimScoreAsync(Claim claim);
    }
}