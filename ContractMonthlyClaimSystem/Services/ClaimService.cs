using System;
using System.Collections.Generic;
using System.Linq;
using CMCS.Models;

namespace CMCS.Services
{
    public class ClaimService
    {
        private readonly DataService _dataService;

        public ClaimService(DataService dataService)
        {
            _dataService = dataService;
        }

        public (bool Success, string Message) ValidateClaim(Claim claim)
        {
            // Validation checks
            if (claim.HoursWorked <= 0)
                return (false, "Hours worked must be greater than 0");

            if (claim.HourlyRate <= 0)
                return (false, "Hourly rate must be greater than 0");

            if (claim.HoursWorked > 200)
                return (false, "Hours worked cannot exceed 200 hours per month");

            // Check for duplicate claims in the same month
            var existingClaims = _dataService.GetClaimsByLecturer(claim.LecturerId);
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var duplicateClaim = existingClaims.FirstOrDefault(c =>
                c.SubmissionDate.Month == currentMonth &&
                c.SubmissionDate.Year == currentYear);

            if (duplicateClaim != null)
                return (false, "A claim for this month has already been submitted");

            return (true, "Valid");
        }

        public void ProcessClaim(Claim claim)
        {
            // Auto-calculate total
            claim.CalculateTotal();

            // Add the claim
            _dataService.AddClaim(claim);
        }
    }
}