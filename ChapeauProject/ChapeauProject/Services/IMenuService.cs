using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public interface IMenuService
    {
        MenuItem GetById(int menuItemID);

      
        List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter);

        List<MenuItem> GetMenuForWaiter(string menuCard, string courseFilter);
    }
}