using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Services;
using System.Linq;

namespace CMCS.Controllers
{
    public class ManagerController : Controller
    {
        private readonly DataService _dataService;

        public ManagerController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();

            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            ViewBag.TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);

            return View();
        }

        public IActionResult ReviewClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("Login", "Account");

            return View(_dataService.GetClaims());
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Approved");
            TempData["Success"] = "Claim approved!";
            return RedirectToAction("ReviewClaims");
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Rejected");
            TempData["Warning"] = "Claim rejected.";
            return RedirectToAction("ReviewClaims");
        }

        public IActionResult Reports()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();

            var summary = claims
                .GroupBy(c => new { c.SubmissionDate.Year, c.SubmissionDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalClaims = g.Count(),
                    TotalApproved = g.Count(c => c.Status == "Approved"),
                    TotalAmount = g.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            return View(summary);
        }
    }
}
