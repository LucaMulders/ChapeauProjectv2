using ChapeauProject.Models;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface ITableService
    {
        List<Table> GetAllTables();
        List<Table> GetAllOccupiedTables();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        TableOrderViewModel GetTableOrders(int tableNumber);
        int GetOrderCount(int tableNumber);
        int GetGuestCount(int tableNumber);
        List<Guest> GetGuestsByTable(int tableNumber);
        (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber);
        void SetFree(int tableNumber);
        void RemoveGuests(int tableNumber);
    }
}