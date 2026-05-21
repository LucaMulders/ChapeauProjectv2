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

        public IActionResult Index(string menuCard = "Lunch", string category = "All")
        {
         
            List<MenuItem> filteredItems = _menuService.GetMenuForWaiter(menuCard, category);

            // Using the ViewModel to pass data to the View
            MenuViewModel viewModel = new MenuViewModel
            {
                MenuItems = filteredItems,
                SelectedCard = menuCard,
                SelectedCategory = category
            };

            return View(viewModel);
        }
    }
}