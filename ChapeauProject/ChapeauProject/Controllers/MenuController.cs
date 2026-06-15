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
            MenuCard card = MenuCard.Lunch;
            Enum.TryParse(menuCard, true, out card);

            List<MenuItem> items = _menuService.GetCourseFiltered(card, category);

            MenuViewModel viewModel = new MenuViewModel
            {
                MenuItems = items,
                SelectedCard = menuCard,
                SelectedCategory = category
            };

            return View(viewModel);
        }
    }
}