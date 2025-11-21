using CMCS.Data;
using CMCS.Models;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ContractMonthlyClaimSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    Email = "lecturer@cmcs.com",
                    DisplayName = "Dr. John Smith",
                    RoleType = "Lecturer",
                    IsActive = true
                },
                new User
                {
                    Email = "coordinator@cmcs.com",
                    DisplayName = "Prof. Sarah Johnson",
                    RoleType = "Coordinator",
                    IsActive = true
                },
                new User
                {
                    Email = "manager@cmcs.com",
                    DisplayName = "Dr. Michael Brown",
                    RoleType = "Manager",
                    IsActive = true
                },
                new User
                {
                    Email = "hr@cmcs.com",
                    DisplayName = "Ms. Emily Davis",
                    RoleType = "HR",
                    IsActive = true
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Seed Lecturer Profile
            var lecturerUser = users.First(u => u.RoleType == "Lecturer");
            var lecturerProfile = new LecturerProfile
            {
                UserId = lecturerUser.UserId,
                EmployeeNumber = "EMP001",
                Faculty = "Faculty of Computing",
                ContactNumber = "012-345-6789",
                DefaultHourlyRate = 150.00m // Correct property name
            };

            await context.LecturerProfiles.AddAsync(lecturerProfile);
            await context.SaveChangesAsync();

            // Seed Sample Claims for Testing
            var sampleClaims = new List<Claim>
            {
                new Claim
                {
                    LecturerProfileId = lecturerProfile.LecturerProfileId,
                    ClaimMonth = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1),
                    HoursWorked = 120,
                    HourlyRate = 150.00m,
                    TotalAmount = 18000.00m,
                    Status = "Pending",
                    SubmissionDate = DateTime.Now.AddDays(-5),
                    Notes = "October teaching hours"
                },
                new Claim
                {
                    LecturerProfileId = lecturerProfile.LecturerProfileId,
                    ClaimMonth = new DateTime(DateTime.Now.AddMonths(-2).Year, DateTime.Now.AddMonths(-2).Month, 1),
                    HoursWorked = 110,
                    HourlyRate = 150.00m,
                    TotalAmount = 16500.00m,
                    Status = "Approved",
                    SubmissionDate = DateTime.Now.AddDays(-35),
                    CoordinatorApprovalDate = DateTime.Now.AddDays(-33),
                    ManagerApprovalDate = DateTime.Now.AddDays(-32),
                    Notes = "September teaching hours"
                }
            };

            await context.Claims.AddRangeAsync(sampleClaims);
            await context.SaveChangesAsync();
        }
    }
}
