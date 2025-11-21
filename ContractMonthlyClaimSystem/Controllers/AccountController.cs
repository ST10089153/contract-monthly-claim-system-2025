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
    }
}