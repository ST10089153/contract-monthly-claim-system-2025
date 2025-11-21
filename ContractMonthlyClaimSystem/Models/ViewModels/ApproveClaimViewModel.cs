using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ApproveClaimViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Action is required")]
        public string Action { get; set; } = string.Empty; // "Approve" or "Reject"

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}