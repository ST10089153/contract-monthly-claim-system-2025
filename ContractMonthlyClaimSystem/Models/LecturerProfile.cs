using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public class LecturerProfile
    {
        [Key]
        public int LecturerProfileId { get; set; }

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(200)]
        public string Email { get; set; }

        [Required]
        public decimal DefaultHourlyRate { get; set; }

        [MaxLength(20)]
        public string EmployeeNumber { get; set; }

        [MaxLength(200)]
        public string Faculty { get; set; }

        [MaxLength(20)]
        public string ContactNumber { get; set; }

        public ICollection<Claim> Claims { get; set; } = new List<Claim>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
