using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Services;
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

        public IActionResult ManageLecturers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "HR")
                return RedirectToAction("Login", "Account");

            var lecturers = _dataService.GetUsers()
                .Where(u => u.Role == "Lecturer")
                .ToList();

            return View(lecturers);
        }
    }
}