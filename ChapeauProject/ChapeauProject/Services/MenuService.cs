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

        public MenuItem GetById(int menuItemID)
        {
            return _menuRepository.GetById(menuItemID);
        }

        public List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter)
        {
            return _menuRepository.GetCourseFiltered(cardFilter, courseFilter);
        }

        public List<MenuItem> GetMenuItemForWaiter(string menuCard, string courseFilter)
        {
            MenuCard parsedCard;

          
            if (Enum.TryParse(menuCard, true, out parsedCard))
            {
                return _menuRepository.GetCourseFiltered(parsedCard, courseFilter);
            }
            else
            {
                // default
                return _menuRepository.GetCourseFiltered(MenuCard.Lunch, courseFilter);
            }
        }
    }
}