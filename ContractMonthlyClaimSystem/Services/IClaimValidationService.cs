using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimModel = CMCS.Models.Claim;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IClaimValidationService
    {
        Task<(bool IsValid, List<string> Errors)> ValidateClaimAsync(ClaimModel claim);
        Task<bool> CheckDuplicateClaimAsync(int lecturerProfileId, DateTime claimMonth);
        Task<bool> ValidateHoursWorkedAsync(decimal hours);
        Task<bool> ValidateHourlyRateAsync(decimal rate);
        Task<List<string>> GetValidationErrorsAsync(ClaimModel claim);
    }
}
