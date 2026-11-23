using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Services;
using System.Linq;

namespace CMCS.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly DataService _dataService;

        public CoordinatorController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims();

            ViewBag.PendingCount = claims.Count(c => c.Status == "Pending");
            ViewBag.TotalCount = claims.Count;

            return View();
        }

        public IActionResult ReviewClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetClaims()
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.SubmissionDate)
                .ToList();

            return View(claims);
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Approved");
            TempData["Success"] = "Claim approved!";
            return RedirectToAction("ReviewClaims");
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, "Rejected");
            TempData["Warning"] = "Claim rejected.";
            return RedirectToAction("ReviewClaims");
        }

        public IActionResult AllClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("Login", "Account");

            return View(_dataService.GetClaims());
        }
    }
}
