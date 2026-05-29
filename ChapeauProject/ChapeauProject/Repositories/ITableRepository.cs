using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface ITableRepository
    {
        List<Table> GetAllTables();
        List<Table> GetAllOccupiedTables();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        List<GuestOrder> GetTableOrders(int tableNumber);
        int GetOrderCount(int tableNumber);
        int GetGuestCount(int tableNumber);
        List<(int GuestID, string GuestName)> GetGuestsByTable(int tableNumber);
        (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber);
        void SetFree(int tableNumber);
    }
}