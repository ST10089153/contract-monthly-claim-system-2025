using System;
using System.Collections.Generic;

namespace CMCS.Models.ViewModels
{
    public class MonthlySummaryViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalClaims { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalPending { get; set; }
        public int TotalFlagged { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public DateTime GeneratedDate { get; set; }
        public List<LecturerClaimSummary> ClaimsByLecturer { get; set; }
    }

    public class LecturerClaimSummary
    {
        public string LecturerName { get; set; }
        public int ClaimCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class LecturerPerformanceViewModel
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageHoursPerClaim { get; set; }
        public decimal ApprovalRate { get; set; }
        public List<Claim> Claims { get; set; }
    }
}