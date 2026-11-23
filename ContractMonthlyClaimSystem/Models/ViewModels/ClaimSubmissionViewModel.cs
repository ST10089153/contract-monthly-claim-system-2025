using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CMCS.Models.ViewModels
{
    public class ClaimSubmissionViewModel
    {
        [Required]
        [Range(0.1, 1000, ErrorMessage = "Hours must be between 0.1 and 1000")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Hourly rate must be between 1 and 10000")]
        public decimal HourlyRate { get; set; }

        public string? Notes { get; set; }

        public IFormFile? SupportingDocument { get; set; }

        public decimal TotalAmount => HoursWorked * HourlyRate;
    }
}