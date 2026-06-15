using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChapeauProject.Controllers
{
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
                MenuCard card = Enum.TryParse<MenuCard>(menuCard, true, out var parsed) ? parsed : MenuCard.Lunch;
                List<MenuItem> filteredItems = _menuService.GetCourseFiltered(card, category);

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