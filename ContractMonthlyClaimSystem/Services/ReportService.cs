using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CMCS.Data;
using CMCS.Models.ViewModels;

namespace CMCS.Services
{
    public class ReportingService
    {
        private readonly ApplicationDbContext _context;

        public ReportingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Generate monthly summary report
        public async Task<MonthlySummaryViewModel> GenerateMonthlySummaryAsync(int month, int year)
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.SubmissionDate.Month == month &&
                           c.SubmissionDate.Year == year)
                .ToListAsync();

            var summary = new MonthlySummaryViewModel
            {
                Month = month,
                Year = year,
                TotalClaims = claims.Count,
                TotalApproved = claims.Count(c => c.Status == "Approved"),
                TotalRejected = claims.Count(c => c.Status == "Rejected"),
                TotalPending = claims.Count(c => c.Status == "Pending"),
                TotalFlagged = claims.Count(c => c.Status == "Flagged"),
                TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                AverageClaimAmount = claims.Any() ? claims.Average(c => c.TotalAmount) : 0,
                GeneratedDate = DateTime.Now,
                ClaimsByLecturer = claims.GroupBy(c => c.Lecturer.FullName)
                    .Select(g => new LecturerClaimSummary
                    {
                        LecturerName = g.Key,
                        ClaimCount = g.Count(),
                        TotalAmount = g.Sum(c => c.TotalAmount)
                    }).ToList()
            };

            return summary;
        }

        // Generate lecturer performance report
        public async Task<LecturerPerformanceViewModel> GenerateLecturerReportAsync(int lecturerId)
        {
            var lecturer = await _context.Users.FindAsync(lecturerId);
            if (lecturer == null)
                return null;

            var claims = await _context.Claims
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            var report = new LecturerPerformanceViewModel
            {
                LecturerId = lecturerId,
                LecturerName = lecturer.FullName,
                TotalClaims = claims.Count,
                TotalHoursWorked = claims.Sum(c => c.HoursWorked),
                TotalEarnings = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                AverageHoursPerClaim = claims.Any() ? claims.Average(c => c.HoursWorked) : 0,
                ApprovalRate = claims.Any() ?
                    (decimal)claims.Count(c => c.Status == "Approved") / claims.Count * 100 : 0,
                Claims = claims
            };

            return report;
        }
    }
}