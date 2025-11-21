using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class SubmitClaimViewModel
    {
        [Required(ErrorMessage = "Claim month is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Claim Month")]
        public DateTime ClaimMonth { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 744, ErrorMessage = "Hours must be between 1 and 744")]
        [Display(Name = "Total Hours Worked")]
        public int TotalHours { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(50, 500, ErrorMessage = "Hourly rate must be between R50 and R500")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }

        [Display(Name = "Supporting Documents")]
        public List<IFormFile>? SupportingDocuments { get; set; }

        // Computed property - AUTO CALCULATION
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => TotalHours * HourlyRate;
    }
}