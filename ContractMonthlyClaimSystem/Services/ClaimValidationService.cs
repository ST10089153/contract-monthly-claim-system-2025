using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    using ClaimModel = CMCS.Models.Claim;

    public class ClaimValidationService : IClaimValidationService
    {
        private readonly ApplicationDbContext _context;
        private const decimal MIN_HOURS = 1;
        private const decimal MAX_HOURS = 744; // Max hours in a month
        private const decimal MIN_RATE = 50.00m;
        private const decimal MAX_RATE = 500.00m;

        public ClaimValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, List<string> Errors)> ValidateClaimAsync(ClaimModel claim)
        {
            var errors = await GetValidationErrorsAsync(claim);
            return (errors.Count == 0, errors);
        }

        public async Task<List<string>> GetValidationErrorsAsync(ClaimModel claim)
        {
            var errors = new List<string>();

            if (!await ValidateHoursWorkedAsync(claim.HoursWorked))
                errors.Add($"Hours must be between {MIN_HOURS} and {MAX_HOURS}");

            if (!await ValidateHourlyRateAsync(claim.HourlyRate))
                errors.Add($"Hourly rate must be between R{MIN_RATE} and R{MAX_RATE}");

            if (claim.LecturerProfileId.HasValue &&
                await CheckDuplicateClaimAsync(claim.LecturerProfileId.Value, claim.ClaimMonth))
                errors.Add("A claim for this month already exists");

            var expectedTotal = claim.HoursWorked * claim.HourlyRate;
            if (claim.TotalAmount != expectedTotal)
                errors.Add($"Total amount mismatch. Expected: R{expectedTotal:N2}, Got: R{claim.TotalAmount:N2}");

            if (claim.ClaimMonth > System.DateTime.Now)
                errors.Add("Cannot submit claims for future months");

            return errors;
        }

        public async Task<bool> CheckDuplicateClaimAsync(int lecturerProfileId, System.DateTime claimMonth)
        {
            return await _context.Claims
                .AnyAsync(c => c.LecturerProfileId == lecturerProfileId
                              && c.ClaimMonth.Year == claimMonth.Year
                              && c.ClaimMonth.Month == claimMonth.Month);
        }

        public Task<bool> ValidateHoursWorkedAsync(decimal hours)
        {
            return Task.FromResult(hours >= MIN_HOURS && hours <= MAX_HOURS);
        }

        public Task<bool> ValidateHourlyRateAsync(decimal rate)
        {
            return Task.FromResult(rate >= MIN_RATE && rate <= MAX_RATE);
        }
    }
}
