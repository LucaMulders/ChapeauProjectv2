using ChapeauProject.Models;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface ITableService
    {
        List<Table> GetAllTables();
        List<TablesViewModel> GetTableSummaries(bool occupiedOnly = false);
        List<Table> GetAllOccupiedTables();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        TableOrderViewModel GetTableOrders(int tableNumber);
        List<Guest> GetGuestsByTable(int tableNumber);
        void MarkTableAsFree(int tableNumber);
        void RemoveGuests(int tableNumber);
        Guest CreateUnnamedGuest(int tableNumber);
        void RemoveGuest(int guestId);
    }
}