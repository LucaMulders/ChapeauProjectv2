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

        public List<MenuItem> GetMenuForWaiter(string menuCard, string courseFilter)
        {
           
            if (Enum.TryParse(menuCard, true, out MenuCard parsedCard))
            {
                return _menuRepository.GetCourseFiltered(parsedCard, courseFilter);
            }

            return _menuRepository.GetCourseFiltered(MenuCard.Lunch, courseFilter);
        }
    }
}