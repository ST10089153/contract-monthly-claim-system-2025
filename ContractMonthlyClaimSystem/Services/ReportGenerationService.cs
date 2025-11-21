using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    using ClaimModel = CMCS.Models.Claim;

    public class ReportGenerationService : IReportGenerationService
    {
        private readonly ApplicationDbContext _context;

        public ReportGenerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateClaimsReportAsync(List<ClaimModel> claims)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Reference,Lecturer,Employee Number,Month,Hours,Rate,Amount,Status,Submitted");

            foreach (var claim in claims)
            {
                sb.AppendLine($"{claim.ReferenceNumber}," +
                              $"{claim.LecturerProfile?.User?.DisplayName ?? "N/A"}," +
                              $"{claim.LecturerProfile?.EmployeeNumber ?? "N/A"}," +
                              $"{claim.ClaimMonth:yyyy-MM}," +
                              $"{claim.HoursWorked}," +
                              $"R{claim.HourlyRate:N2}," +
                              $"R{claim.TotalAmount:N2}," +
                              $"{claim.Status}," +
                              $"{claim.SubmissionDate:yyyy-MM-dd}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateInvoiceAsync(ClaimModel claim)
        {
            var sb = new StringBuilder();
            sb.AppendLine("INVOICE");
            sb.AppendLine("================================");
            sb.AppendLine($"Invoice Number: {claim.ReferenceNumber}");
            sb.AppendLine($"Date: {System.DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine($"Lecturer: {claim.LecturerProfile?.User?.DisplayName ?? "N/A"}");
            sb.AppendLine($"Employee Number: {claim.LecturerProfile?.EmployeeNumber ?? "N/A"}");
            sb.AppendLine($"Faculty: {claim.LecturerProfile?.Faculty ?? "N/A"}");
            sb.AppendLine();
            sb.AppendLine("SERVICES PROVIDED");
            sb.AppendLine("================================");
            sb.AppendLine($"Period: {claim.ClaimMonth:MMMM yyyy}");
            sb.AppendLine($"Hours Worked: {claim.HoursWorked} hours");
            sb.AppendLine($"Hourly Rate: R{claim.HourlyRate:N2}");
            sb.AppendLine();
            sb.AppendLine($"TOTAL AMOUNT DUE: R{claim.TotalAmount:N2}");
            sb.AppendLine();
            sb.AppendLine($"Status: {claim.Status}");
            sb.AppendLine($"Approved: {claim.ManagerApprovalDate?.ToString("yyyy-MM-dd") ?? "Pending"}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateMonthlySummaryAsync(int month, int year)
        {
            var claims = await _context.Claims
                .Include(c => c.LecturerProfile)
                .ThenInclude(lp => lp.User)
                .Where(c => c.ClaimMonth.Month == month && c.ClaimMonth.Year == year)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"MONTHLY SUMMARY - {new System.DateTime(year, month, 1):MMMM yyyy}");
            sb.AppendLine("================================");
            sb.AppendLine($"Total Claims: {claims.Count}");
            sb.AppendLine($"Approved: {claims.Count(c => c.Status == "Approved")}");
            sb.AppendLine($"Pending: {claims.Count(c => c.Status == "Pending")}");
            sb.AppendLine($"Rejected: {claims.Count(c => c.Status == "Rejected")}");
            sb.AppendLine();
            sb.AppendLine($"Total Hours: {claims.Sum(c => c.HoursWorked)}");
            sb.AppendLine($"Total Amount (Approved): R{claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount):N2}");
            sb.AppendLine();
            sb.AppendLine("DETAILS");
            sb.AppendLine("================================");

            foreach (var claim in claims)
            {
                sb.AppendLine($"{claim.LecturerProfile?.User?.DisplayName ?? "N/A"} - " +
                              $"R{claim.TotalAmount:N2} - {claim.Status}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
