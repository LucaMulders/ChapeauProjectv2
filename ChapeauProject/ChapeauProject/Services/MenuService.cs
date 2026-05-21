using ChapeauProject.Models;
using ChapeauProject.Repositories;
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
        //used to get the menu 
        public List<MenuItem> GetMenuForWaiter(string menuCard, string courseFilter)
        {
           
            return _menuRepository.GetFiltered(menuCard, courseFilter);
        }
    }
}