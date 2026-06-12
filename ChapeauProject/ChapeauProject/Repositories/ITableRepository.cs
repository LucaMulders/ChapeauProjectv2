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
        List<Guest> GetGuestsByTable(int tableNumber);
        OrderCategories GetRunningOrderCategories(int tableNumber);
        void MarkTableAsFree(int tableNumber);
        void RemoveGuests(int tableNumber);
        int InsertGuest(int tableNumber, string firstName, string lastName);
    }
}