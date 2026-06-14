using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChapeauProject.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public IActionResult Index(string menuCard = nameof(MenuCard.Lunch), string category = CourseFilter.All)
        {
            try
            {
                List<MenuItem> filteredItems = _menuService.GetCourseFilteredByName(menuCard, category);

                MenuViewModel viewModel = new MenuViewModel
                {
                    MenuItems        = filteredItems,
                    SelectedCard     = menuCard,
                    SelectedCategory = category
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Changed errors to be more generic to avoid giving away information about the system

                Console.Error.WriteLine($"[MenuController.Index] {ex}");
                TempData["ErrorMessage"] = "Failed to load menu. Please try again.";
                return RedirectToAction("Index", "Tables");
            }
        }
    }
}