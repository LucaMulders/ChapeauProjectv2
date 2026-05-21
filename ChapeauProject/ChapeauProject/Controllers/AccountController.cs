using ChapeauProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ChapeauProject.Services;
using System.Security.Claims;

namespace ChapeauProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly IStaffService _staffService;

        public AccountController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        private async Task SignInStaff(Staff staff)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, staff.StaffID.ToString()),
                new Claim(ClaimTypes.Name, staff.FirstName + " " + staff.LastName),
                new Claim(ClaimTypes.Role, staff.Role)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel loginModel)
        {
            Staff? staff = _staffService.GetByLoginCredentials(loginModel.StaffID, loginModel.Password);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "Invalid staffID or password";
                return View(loginModel);
            }

            await SignInStaff(staff);
            HttpContext.Session.SetObject("LoggedInStaff", staff);
            TempData["SuccessMessage"] = "Welcome back, " + staff.FirstName + "!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logoff
        [HttpGet]
        public async Task<ActionResult> Logoff()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("LoggedInStaff");
            return RedirectToAction("Login");
        }
    }
}