using ChapeauProject.Models;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface ITableService
    {
        List<Table> GetAll();
        Table? GetByTableNumber(int tableNumber);
        void ToggleOccupied(int tableNumber);
        TableOrderViewModel GetTableOrders(int tableNumber);
        int GetOrderCount(int tableNumber);
    }
}