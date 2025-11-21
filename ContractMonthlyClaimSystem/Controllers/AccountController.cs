using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Models.ViewModels;
using CMCS.Services;

namespace CMCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataService _dataService;

        public AccountController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _dataService.GetUser(model.Username, model.Password);
                if (user != null)
                {
                    // Check if user is active
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError("", "Your account has been deactivated. Please contact HR.");
                        return View(model);
                    }

                    // Set session variables
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("FullName", user.FullName);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid username or password");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ============== REGISTRATION DISABLED ==============
        // HR creates all user accounts

        public IActionResult Register()
        {
            // Redirect to login with message
            TempData["Info"] = "User registration is managed by HR. Please contact HR to create an account.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Register(object model)
        {
            // Redirect any POST attempts to login
            TempData["Info"] = "User registration is managed by HR. Please contact HR to create an account.";
            return RedirectToAction("Login");
        }
    }
}