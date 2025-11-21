using CMCS.Models;
using CMCS.Services;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CMCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService _dataService;

        public HomeController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole))
                return RedirectToAction("Login", "Account");

            // Redirect to role-specific dashboard
            return userRole switch
            {
                "Lecturer" => RedirectToAction("Index", "Lecturer"),
                "Coordinator" => RedirectToAction("Index", "Coordinator"),
                "Manager" => RedirectToAction("Index", "Manager"),
                "HR" => RedirectToAction("Index", "HR"),
                _ => View()  // Changed from RedirectToAction("Index") to View()
            };
        }

        // REMOVED Dashboard action - not needed since Index handles everything

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}