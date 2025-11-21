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
    public class LecturerController : Controller
    {
        private readonly DataService _dataService;
        private readonly ClaimService _claimService;
        private readonly ValidationService _validationService;
        private readonly IWebHostEnvironment _environment;

        public LecturerController(
            DataService dataService,
            ClaimService claimService,
            ValidationService validationService,
            IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _claimService = claimService;
            _validationService = validationService;
            _environment = environment;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var claims = _dataService.GetClaimsByLecturer(userId);

            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");

            return View();
        }

        public IActionResult SubmitClaim()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("Login", "Account");

            // Validate supporting document
            if (model.SupportingDocument != null)
            {
                var fileValidation = _validationService.ValidateFile(model.SupportingDocument);

                if (!fileValidation.IsValid)
                {
                    ModelState.AddModelError("SupportingDocument", fileValidation.Message);
                    return View(model);
                }
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var fullName = HttpContext.Session.GetString("FullName");

            var claim = new Claim
            {
                LecturerId = userId,
                LecturerName = fullName,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                SubmissionDate = DateTime.Now,
                ClaimMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };

            // Validate claim business rules
            var validation = _claimService.ValidateClaim(claim);
            if (!validation.Success)
            {
                ModelState.AddModelError("", validation.Message);
                return View(model);
            }

            // File upload
            if (model.SupportingDocument != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{model.SupportingDocument.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await model.SupportingDocument.CopyToAsync(fileStream);

                claim.Documents.Add(new Document
                {
                    FileName = model.SupportingDocument.FileName,
                    FilePath = uniqueFileName,
                    FileSize = model.SupportingDocument.Length,
                    UploadDate = DateTime.Now
                });
            }

            _claimService.ProcessClaim(claim);

            TempData["Success"] = "Claim submitted successfully!";
            return RedirectToAction("ViewClaims");
        }

        public IActionResult ViewClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var claims = _dataService.GetClaimsByLecturer(userId);

            return View(claims);
        }

        public IActionResult TrackClaim(int id)
        {
            var claim = _dataService.GetClaim(id);
            if (claim == null)
                return NotFound();

            return View(claim);
        }
    }
}
