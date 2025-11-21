using System;
using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class ApprovalHistory
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public int ApproverId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string Comments { get; set; }

        public DateTime ActionDate { get; set; }

        // Navigation properties
        public virtual Claim Claim { get; set; }
        public virtual User Approver { get; set; }
    }
}