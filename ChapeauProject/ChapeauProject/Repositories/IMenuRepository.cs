using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IMenuRepository
    {
        
        List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter);
        MenuItem? GetById(int menuItemID);
        void DeductStockQuantity(int menuItemID, int amountToDeduct);
    }
}