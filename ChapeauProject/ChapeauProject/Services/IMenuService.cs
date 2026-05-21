using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public interface IMenuService
    {
        List<MenuItem> GetMenuForWaiter(string menuCard, string courseFilter);
    }
}
//menu not menu item