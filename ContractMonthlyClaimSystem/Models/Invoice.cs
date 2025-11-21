using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; }

        public DateTime GeneratedDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Generated";

        // Navigation property
        public virtual Claim Claim { get; set; }
    }
}