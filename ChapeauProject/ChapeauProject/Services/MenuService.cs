using ChapeauProject.Models;
using ChapeauProject.Repositories;
using System;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public MenuItem? GetMenuItemById(int menuItemID)
        {
            return _menuRepository.GetMenuItemById(menuItemID);
        }

        public List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter)
        {
            return _menuRepository.GetCourseFiltered(cardFilter, courseFilter);
        }


        public List<MenuItem> GetCourseFilteredByName(string menuCard, string courseFilter)
        {
            if (Enum.TryParse(menuCard, true, out MenuCard parsed))
                return GetCourseFiltered(parsed, courseFilter);
            else
                return GetCourseFiltered(MenuCard.Lunch, courseFilter);
        }

    }
}