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

        //NOTE Can we prevent the webpage from refreshing when you add a new item? It is kinda annoying
        public IActionResult Index(string menuCard = "Lunch", string category = "All")
        {
            try
            {
                List<MenuItem> filteredItems = _menuService.GetMenuItemForWaiter(menuCard, category);

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