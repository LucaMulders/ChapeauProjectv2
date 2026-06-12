using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChapeauProject.Services;
using System.Security.Claims;

namespace ChapeauProject.Controllers
{
    [AllowAnonymous]
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
                new Claim(ClaimTypes.Role, staff.Role.ToString())
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginModel)
        {
            try
            {
                Staff? staff = _staffService.GetByLoginCredentials(loginModel.StaffID, loginModel.Password);

                if (staff == null)
                {
                    ViewBag.ErrorMessage = "Invalid staffID or password";
                    return View(loginModel);
                }

                await SignInStaff(staff);
                staff.Password = string.Empty; 
                HttpContext.Session.SetObject("LoggedInStaff", staff);
                TempData["SuccessMessage"] = "Welcome back, " + staff.FirstName + "!";
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login: " + ex.Message;
                return View(loginModel);
            }
        }

        // GET: /Account/Logoff
        [HttpGet]
        public async Task<ActionResult> Logoff()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Remove("LoggedInStaff");
            }
            catch (Exception ex)
            {
                // continue with logoff even if sign out fails, to ensure user is logged out
                Console.Error.WriteLine($"[Logoff] Sign-out error: {ex.Message}");
            }
            return RedirectToAction("Login");
        }
    }
}