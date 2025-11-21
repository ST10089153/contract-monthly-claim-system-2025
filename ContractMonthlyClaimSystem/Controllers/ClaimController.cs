using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using CMCS.Models;
using CMCS.Models.ViewModels;
using CMCS.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS.Controllers
{
    public class ClaimController : Controller
    {
        private readonly DataService _dataService;
        private readonly IWebHostEnvironment _environment;
        private const decimal MAX_HOURS_PER_MONTH = 180m; // Maximum hours allowed per month

        public ClaimController(DataService dataService, IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _environment = environment;
        }

        // Lecturer Actions
        public IActionResult Submit()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Lecturer")
                return RedirectToAction("Login", "Account");

            // Get lecturer's hourly rate from user profile
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = _dataService.GetUserById(userId);

            if (user != null)
            {
                ViewBag.HourlyRate = user.HourlyRate;
                ViewBag.LecturerName = user.FullName;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ClaimSubmissionViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = _dataService.GetUserById(userId);

            if (ModelState.IsValid)
            {
                var fullName = HttpContext.Session.GetString("FullName");

                // NEW: Validate monthly hours don't exceed 180
                var currentMonth = DateTime.Now;
                var existingClaims = _dataService.GetClaimsByLecturer(userId)
                    .Where(c => c.ClaimMonth.Year == currentMonth.Year
                             && c.ClaimMonth.Month == currentMonth.Month
                             && c.Status != "Rejected")
                    .ToList();

                var totalHoursThisMonth = existingClaims.Sum(c => c.HoursWorked);
                var newTotal = totalHoursThisMonth + model.HoursWorked;

                if (newTotal > MAX_HOURS_PER_MONTH)
                {
                    ModelState.AddModelError("HoursWorked",
                        $"Total hours for this month would be {newTotal:F2}, which exceeds the maximum of {MAX_HOURS_PER_MONTH} hours. " +
                        $"You have already claimed {totalHoursThisMonth:F2} hours this month.");

                    ViewBag.HourlyRate = user?.HourlyRate ?? 0;
                    ViewBag.LecturerName = user?.FullName ?? fullName;
                    return View(model);
                }

                var claim = new Claim
                {
                    LecturerId = userId,
                    LecturerName = fullName,
                    ClaimMonth = DateTime.Now, // Set to current month
                    HoursWorked = model.HoursWorked,
                    HourlyRate = user?.HourlyRate ?? model.HourlyRate, // Use user's hourly rate
                    TotalAmount = model.HoursWorked * (user?.HourlyRate ?? model.HourlyRate),
                    Notes = model.Notes,
                    ReferenceNumber = GenerateReferenceNumber(),
                    Documents = new List<Document>() // Ensure not null
                };

                // Handle file upload
                if (model.SupportingDocument != null)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.SupportingDocument.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SupportingDocument.CopyToAsync(fileStream);
                    }

                    claim.Documents.Add(new Document
                    {
                        FileName = model.SupportingDocument.FileName,
                        FilePath = uniqueFileName,
                        FileSize = model.SupportingDocument.Length,
                        UploadDate = DateTime.Now
                    });
                }

                _dataService.AddClaim(claim);
                TempData["Success"] = $"Claim submitted successfully! You have used {newTotal:F2} of {MAX_HOURS_PER_MONTH} hours this month.";

                return RedirectToAction("MyClaims", "Claim");
            }

            ViewBag.HourlyRate = user?.HourlyRate ?? 0;
            ViewBag.LecturerName = user?.FullName ?? "";
            return View(model);
        }

        public IActionResult MyClaims()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Lecturer")
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var claims = _dataService.GetClaimsByLecturer(userId);

            return View(claims);
        }

        // Coordinator/Manager Actions
        public IActionResult Review()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Coordinator" && userRole != "Manager")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();
            return View(claims);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Coordinator" && userRole != "Manager")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Approved");
            TempData["Success"] = "Claim approved successfully!";
            return RedirectToAction("Review");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Coordinator" && userRole != "Manager")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Rejected");
            TempData["Warning"] = "Claim rejected!";
            return RedirectToAction("Review");
        }

        // Track claim status
        public IActionResult Track(int? id)
        {
            if (id == null)
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var claims = _dataService.GetClaimsByLecturer(userId);
                return View("TrackList", claims);
            }

            var claim = _dataService.GetClaim(id.Value);
            if (claim == null)
                return NotFound();

            return View(claim);
        }

        // Helper method to generate unique reference number
        private string GenerateReferenceNumber()
        {
            return $"CLM-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}