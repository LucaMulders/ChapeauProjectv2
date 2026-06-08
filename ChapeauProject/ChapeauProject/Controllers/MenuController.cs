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

        public IActionResult Index(string menuCard = "Lunch", string category = CourseFilter.All)
        {
            try
            {
                MenuCard card = Enum.TryParse(menuCard, true, out MenuCard parsed) ? parsed : MenuCard.Lunch;
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
                TempData["ErrorMessage"] = "Failed to load menu: " + ex.Message;
                return RedirectToAction("Index", "Tables");
            }
        }
    }
}