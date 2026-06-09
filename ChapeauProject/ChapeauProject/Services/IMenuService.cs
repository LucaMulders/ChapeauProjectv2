using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public interface IMenuService
    {
        MenuItem? GetMenuItemById(int menuItemID);
        List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter);
        List<MenuItem> GetCourseFilteredByName(string menuCard, string courseFilter);
        void DeductStockQuantity(int menuItemID, int amountToDeduct);
    }
}