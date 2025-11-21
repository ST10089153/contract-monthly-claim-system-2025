using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        // Link to User (Lecturer)
        [Required]
        public int LecturerId { get; set; }
        public virtual User Lecturer { get; set; }

        // Link to LecturerProfile (optional)
        public int? LecturerProfileId { get; set; }
        public virtual LecturerProfile LecturerProfile { get; set; }

        // Month of the claim
        public DateTime ClaimMonth { get; set; }

        // Financial details
        [Column(TypeName = "decimal(10,2)")]
        public decimal HoursWorked { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime SubmissionDate { get; set; }

        public DateTime? CoordinatorApprovalDate { get; set; }
        public DateTime? ManagerApprovalDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation collections
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();

        // Not persisted helpers
        [NotMapped]
        public string LecturerName { get; set; }

        // **Add this property**
        [Required, MaxLength(50)]
        public string ReferenceNumber { get; set; }

        // Ensure calculation exists for all call sites
        public void CalculateTotal()
        {
            TotalAmount = HoursWorked * HourlyRate;
        }
    }
}
