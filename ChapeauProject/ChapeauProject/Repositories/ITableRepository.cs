using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface ITableRepository
    {
        List<Table> GetAllTables();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        List<GuestOrder> GetTableOrders(int tableNumber);
        int GetOrderCount(int tableNumber);
        int GetGuestCount(int tableNumber);
        (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber);
        void SetFree(int tableNumber);
    }
}