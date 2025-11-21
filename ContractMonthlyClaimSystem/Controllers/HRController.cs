using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Services;
using CMCS.Models;
using System;
using System.Linq;

namespace CMCS.Controllers
{
    public class HRController : Controller
    {
        private readonly DataService _dataService;

        public HRController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();

            var approvedClaims = claims.Count(c => c.Status == "Approved");
            var pendingClaims = claims.Count(c => c.Status == "Pending");
            var totalPayments = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);

            ViewBag.ApprovedClaims = approvedClaims;
            ViewBag.PendingClaims = pendingClaims;
            ViewBag.TotalPayments = totalPayments;
            ViewBag.TotalClaims = claims.Count;

            return View();
        }

        public IActionResult ViewInvoices()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            // Get all approved claims (these are essentially our "invoices")
            var approvedClaims = _dataService.GetClaims()
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            return View(approvedClaims);
        }

        public IActionResult GenerateInvoices()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            // Get all approved claims
            var approvedClaims = _dataService.GetClaims()
                .Where(c => c.Status == "Approved")
                .ToList();

            return View(approvedClaims);
        }

        [HttpPost]
        public IActionResult MarkAsPaid(int claimId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(claimId, "Paid");
            TempData["Success"] = "Claim marked as paid!";
            return RedirectToAction("ViewInvoices");
        }

        public IActionResult MonthlyReport()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var monthlyData = claims
                .Where(c => c.SubmissionDate.Year == currentYear && c.SubmissionDate.Month == currentMonth)
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .ToList();

            ViewBag.Month = DateTime.Now.ToString("MMMM yyyy");
            ViewBag.TotalClaims = monthlyData.Sum(d => d.Count);
            ViewBag.TotalAmount = monthlyData.Sum(d => d.TotalAmount);

            return View(monthlyData);
        }

        // ============== USER MANAGEMENT SECTION ==============

        /// <summary>
        /// View all users (Lecturers, Coordinators, Managers)
        /// </summary>
        public IActionResult ManageLecturers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            // Get all users except HR
            var users = _dataService.GetAllUsers()
                .Where(u => u.Role != "HR")
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToList();

            return View(users);
        }

        /// <summary>
        /// Show form to create a new user
        /// </summary>
        public IActionResult CreateUser()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        /// <summary>
        /// Create a new user (POST)
        /// </summary>
        [HttpPost]
        public IActionResult CreateUser(User newUser, string password)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(newUser.FirstName) ||
                    string.IsNullOrWhiteSpace(newUser.LastName) ||
                    string.IsNullOrWhiteSpace(newUser.Email) ||
                    string.IsNullOrWhiteSpace(newUser.Username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    TempData["Error"] = "All fields are required.";
                    return View(newUser);
                }

                // Check if username already exists
                if (_dataService.UsernameExists(newUser.Username))
                {
                    TempData["Error"] = "Username already exists. Please choose a different username.";
                    return View(newUser);
                }

                // Check if email already exists
                if (_dataService.EmailExists(newUser.Email))
                {
                    TempData["Error"] = "Email already exists. Please use a different email.";
                    return View(newUser);
                }

                // Validate hourly rate for lecturers
                if (newUser.Role == "Lecturer" && newUser.HourlyRate <= 0)
                {
                    TempData["Error"] = "Hourly rate must be greater than zero for lecturers.";
                    return View(newUser);
                }

                // For non-lecturers, set hourly rate to 0
                if (newUser.Role != "Lecturer")
                {
                    newUser.HourlyRate = 0;
                }

                // Set additional properties
                newUser.Password = password; // In production, hash this!
                newUser.CreatedDate = DateTime.Now;
                newUser.IsActive = true;

                // Create user in database
                _dataService.CreateUser(newUser);

                TempData["Success"] = $"User '{newUser.FullName}' created successfully! Login credentials: {newUser.Username} / {password}";
                return RedirectToAction("ManageLecturers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
                return View(newUser);
            }
        }

        /// <summary>
        /// Show form to edit an existing user
        /// </summary>
        public IActionResult EditUser(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            var user = _dataService.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageLecturers");
            }

            return View(user);
        }

        /// <summary>
        /// Update an existing user (POST)
        /// </summary>
        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(updatedUser.FirstName) ||
                    string.IsNullOrWhiteSpace(updatedUser.LastName) ||
                    string.IsNullOrWhiteSpace(updatedUser.Email))
                {
                    TempData["Error"] = "First name, last name, and email are required.";
                    return View(updatedUser);
                }

                // Get existing user
                var existingUser = _dataService.GetUserById(updatedUser.UserId);
                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageLecturers");
                }

                // Check if email changed and if new email already exists
                if (existingUser.Email != updatedUser.Email && _dataService.EmailExists(updatedUser.Email))
                {
                    TempData["Error"] = "Email already exists. Please use a different email.";
                    return View(updatedUser);
                }

                // Validate hourly rate for lecturers
                if (updatedUser.Role == "Lecturer" && updatedUser.HourlyRate <= 0)
                {
                    TempData["Error"] = "Hourly rate must be greater than zero for lecturers.";
                    return View(updatedUser);
                }

                // For non-lecturers, set hourly rate to 0
                if (updatedUser.Role != "Lecturer")
                {
                    updatedUser.HourlyRate = 0;
                }

                // Update user in database
                _dataService.UpdateUser(updatedUser);

                TempData["Success"] = $"User '{updatedUser.FullName}' updated successfully!";
                return RedirectToAction("ManageLecturers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating user: {ex.Message}";
                return View(updatedUser);
            }
        }

        /// <summary>
        /// Deactivate a user
        /// </summary>
        [HttpPost]
        public IActionResult DeactivateUser(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            try
            {
                var user = _dataService.GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageLecturers");
                }

                user.IsActive = false;
                _dataService.UpdateUser(user);

                TempData["Success"] = $"User '{user.FullName}' deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating user: {ex.Message}";
            }

            return RedirectToAction("ManageLecturers");
        }

        /// <summary>
        /// Activate a user
        /// </summary>
        [HttpPost]
        public IActionResult ActivateUser(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            try
            {
                var user = _dataService.GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageLecturers");
                }

                user.IsActive = true;
                _dataService.UpdateUser(user);

                TempData["Success"] = $"User '{user.FullName}' activated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error activating user: {ex.Message}";
            }

            return RedirectToAction("ManageLecturers");
        }

        /// <summary>
        /// Delete a user (optional - use with caution)
        /// </summary>
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            try
            {
                var user = _dataService.GetUserById(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageLecturers");
                }

                // Check if user has any claims
                var hasClaims = _dataService.GetClaims().Any(c => c.LecturerId == id);
                if (hasClaims)
                {
                    TempData["Error"] = "Cannot delete user with existing claims. Consider deactivating instead.";
                    return RedirectToAction("ManageLecturers");
                }

                _dataService.DeleteUser(id);

                TempData["Success"] = $"User '{user.FullName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("ManageLecturers");
        }
    }
}