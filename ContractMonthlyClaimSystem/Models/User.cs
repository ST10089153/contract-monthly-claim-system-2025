using System;

namespace CMCS.Models
{
    public class User
    {
        public int UserId { get; set; }

        // Existing properties
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        // UPDATED to match Seeder
        public string RoleType { get; set; } // Lecturer, Coordinator, Manager, HR

        // Optional but you had "Role" – keep both if needed
        public string Role { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Seeder requires this:
        public string DisplayName { get; set; }

        public bool IsActive { get; set; } = true;

        // Convenience property
        public string FullName => $"{FirstName} {LastName}";
    }

    public class Lecturer : User
    {
        public decimal HourlyRate { get; set; }
        public string Department { get; set; }
    }
}
