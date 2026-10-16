using Microsoft.AspNetCore.Mvc;

namespace SmartClinicSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
                return RedirectToAction("Dashboard", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var adminUser = _config["AdminCredentials:Username"];
            var adminPass = _config["AdminCredentials:Password"];

            if (username == adminUser && password == adminPass)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminName", username);
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
