using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IMenuRepository
    {
        
        List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter);
        MenuItem? GetMenuItemById(int menuItemID);
        void DeductStockQuantity(int menuItemID, int amountToDeduct);
    }
}