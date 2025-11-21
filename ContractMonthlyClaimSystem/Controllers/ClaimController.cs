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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ClaimSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var fullName = HttpContext.Session.GetString("FullName");

                var claim = new Claim
                {
                    LecturerId = userId,
                    LecturerName = fullName,
                    HoursWorked = model.HoursWorked,
                    HourlyRate = model.HourlyRate,
                    TotalAmount = model.HoursWorked * model.HourlyRate,
                    Notes = model.Notes,
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
                TempData["Success"] = "Claim submitted successfully!";

                return RedirectToAction("MyClaims", "Claim");
            }

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
    }
}
