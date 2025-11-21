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

        // Role properties
        public string RoleType { get; set; } // Lecturer, Coordinator, Manager, HR
        public string Role { get; set; }

        // Name properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Status
        public bool IsActive { get; set; } = true;

        // HR MANAGEMENT REQUIREMENT: HourlyRate for ALL users (only lecturers use it)
        public decimal HourlyRate { get; set; } = 0m;

        // Optional - only used by Lecturer subclass if you need it
        public string Department { get; set; }

        // Convenience property
        public string FullName => $"{FirstName} {LastName}";
    }

    // You can still keep this if you need specialized Lecturer functionality
    public class Lecturer : User
    {
        // Inherits HourlyRate from User
        // Can add lecturer-specific properties here if needed
    }
}