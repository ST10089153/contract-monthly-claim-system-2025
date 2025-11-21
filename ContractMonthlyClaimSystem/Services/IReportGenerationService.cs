using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimModel = CMCS.Models.Claim;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IReportGenerationService
    {
        Task<byte[]> GenerateClaimsReportAsync(List<ClaimModel> claims);
        Task<byte[]> GenerateInvoiceAsync(ClaimModel claim);
        Task<byte[]> GenerateMonthlySummaryAsync(int month, int year);
    }
}
