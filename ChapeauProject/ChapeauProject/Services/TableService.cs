using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

        public TableService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public List<Table> GetAll()
        {
            return _tableRepository.GetAll();
        }

        public Table? GetByTableNumber(int tableNumber)
        {
            return _tableRepository.GetByTableNumber(tableNumber);
        }

        public void ToggleOccupied(int tableNumber)
        {
            _tableRepository.ToggleOccupied(tableNumber);
        }

        public TableOrderViewModel GetTableOrders(int tableNumber)
        {
            return _tableRepository.GetTableOrders(tableNumber);
        }

        public int GetOrderCount(int tableNumber)
        {
            return _tableRepository.GetOrderCount(tableNumber);
        }
    }
}