using ChapeauProject.Models;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface ITableService
    {
        List<Table> GetAllTables();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        TableOrderViewModel GetTableOrders(int tableNumber);
        int GetOrderCount(int tableNumber);
        (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber);
        void SetFree(int tableNumber);
    }
}