using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using CMCS.Models;

namespace CMCS.Services
{
    public class DataService
    {
        private readonly string _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        private readonly string _claimsFile;
        private readonly string _usersFile;

        public DataService()
        {
            if (!Directory.Exists(_dataPath))
                Directory.CreateDirectory(_dataPath);

            _claimsFile = Path.Combine(_dataPath, "claims.json");
            _usersFile = Path.Combine(_dataPath, "users.json");

            InitializeData();
        }

        private void InitializeData()
        {
            if (!File.Exists(_usersFile))
            {
                var users = new List<User>
                {
                    new User { UserId = 1, Username = "lecturer1", Password = "password", Email = "lecturer1@example.com",
                             Role = "Lecturer", FirstName = "John", LastName = "Doe", HourlyRate = 350.00m, IsActive = true, CreatedDate = DateTime.Now },
                    new User { UserId = 2, Username = "coordinator1", Password = "password", Email = "coord1@example.com",
                             Role = "Programme Coordinator", FirstName = "Jane", LastName = "Smith", HourlyRate = 0, IsActive = true, CreatedDate = DateTime.Now },
                    new User { UserId = 3, Username = "manager1", Password = "password", Email = "manager1@example.com",
                             Role = "Academic Manager", FirstName = "Mike", LastName = "Johnson", HourlyRate = 0, IsActive = true, CreatedDate = DateTime.Now },
                    new User { UserId = 4, Username = "hr1", Password = "password", Email = "hr1@example.com",
                             Role = "HR", FirstName = "Sarah", LastName = "Williams", HourlyRate = 0, IsActive = true, CreatedDate = DateTime.Now }
                };
                SaveUsers(users);
            }

            if (!File.Exists(_claimsFile))
            {
                SaveClaims(new List<Claim>());
            }
        }

        // ============== EXISTING USER METHODS ==============

        public List<User> GetUsers()
        {
            var json = File.ReadAllText(_usersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public User GetUser(string username, string password)
        {
            var users = GetUsers();
            return users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User GetUserById(int id)
        {
            var users = GetUsers();
            return users.FirstOrDefault(u => u.UserId == id);
        }

        private void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFile, json);
        }

        // ============== NEW HR USER MANAGEMENT METHODS ==============

        /// <summary>
        /// Get all users (for HR management)
        /// </summary>
        public List<User> GetAllUsers()
        {
            return GetUsers();
        }

        /// <summary>
        /// Check if username already exists
        /// </summary>
        public bool UsernameExists(string username)
        {
            var users = GetUsers();
            return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check if email already exists
        /// </summary>
        public bool EmailExists(string email)
        {
            var users = GetUsers();
            return users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Create a new user (HR function)
        /// </summary>
        public void CreateUser(User newUser)
        {
            var users = GetUsers();

            // Auto-generate UserId
            newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;

            // Set defaults
            newUser.CreatedDate = DateTime.Now;
            newUser.IsActive = true;

            users.Add(newUser);
            SaveUsers(users);
        }

        /// <summary>
        /// Update an existing user (HR function)
        /// </summary>
        public void UpdateUser(User updatedUser)
        {
            var users = GetUsers();
            var existingUser = users.FirstOrDefault(u => u.UserId == updatedUser.UserId);

            if (existingUser != null)
            {
                var index = users.IndexOf(existingUser);
                users[index] = updatedUser;
                SaveUsers(users);
            }
        }

        /// <summary>
        /// Delete a user (HR function - use with caution)
        /// </summary>
        public void DeleteUser(int userId)
        {
            var users = GetUsers();
            var user = users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                users.Remove(user);
                SaveUsers(users);
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        public List<User> GetUsersByRole(string role)
        {
            var users = GetUsers();
            return users.Where(u => u.Role == role).ToList();
        }

        // ============== EXISTING CLAIM METHODS ==============

        public List<Claim> GetClaims()
        {
            if (!File.Exists(_claimsFile))
                return new List<Claim>();

            var json = File.ReadAllText(_claimsFile);
            return JsonSerializer.Deserialize<List<Claim>>(json) ?? new List<Claim>();
        }

        public Claim GetClaim(int id)
        {
            var claims = GetClaims();
            return claims.FirstOrDefault(c => c.ClaimId == id);
        }

        public List<Claim> GetClaimsByLecturer(int lecturerId)
        {
            var claims = GetClaims();
            return claims.Where(c => c.LecturerId == lecturerId).ToList();
        }

        public void AddClaim(Claim claim)
        {
            var claims = GetClaims();
            claim.ClaimId = claims.Any() ? claims.Max(c => c.ClaimId) + 1 : 1;
            claim.SubmissionDate = DateTime.Now;
            claim.Status = "Pending";
            claims.Add(claim);
            SaveClaims(claims);
        }

        public void UpdateClaimStatus(int claimId, string status)
        {
            var claims = GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);
            if (claim != null)
            {
                claim.Status = status;
                SaveClaims(claims);
            }
        }

        public void UpdateClaim(Claim updatedClaim)
        {
            var claims = GetClaims();
            var existingClaim = claims.FirstOrDefault(c => c.ClaimId == updatedClaim.ClaimId);
            if (existingClaim != null)
            {
                var index = claims.IndexOf(existingClaim);
                claims[index] = updatedClaim;
                SaveClaims(claims);
            }
        }

        private void SaveClaims(List<Claim> claims)
        {
            var json = JsonSerializer.Serialize(claims, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_claimsFile, json);
        }
    }
}