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

        public List<Table> GetAllTables()
        {
            return _tableRepository.GetAllTables();
        }

        public List<Table> GetAllOccupiedTables()
        {
            return _tableRepository.GetAllOccupiedTables();
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
            var guestOrders = _tableRepository.GetTableOrders(tableNumber);

            var guests = guestOrders.Select(g => new GuestOrderViewModel
            {
                GuestID   = g.GuestID,
                GuestName = g.GuestName,
                Items     = g.Items.Select(i => new OrderItemViewModel
                {
                    OrderItemID       = i.OrderItemID,
                    ItemName          = i.ItemName,
                    Quantity          = i.Quantity,
                    Price             = i.Price,
                    VatRate           = i.VatRate,
                    PreparationStatus = i.PreparationStatus
                }).ToList()
            }).ToList();

            var allItems = guests.SelectMany(g => g.Items).ToList();
            decimal subtotal = allItems.Sum(i => i.Price * i.Quantity);
            decimal lowVat   = allItems.Where(i => i.VatRate == 0.09m).Sum(i => i.Price * i.Quantity * i.VatRate);
            decimal highVat  = allItems.Where(i => i.VatRate == 0.21m).Sum(i => i.Price * i.Quantity * i.VatRate);
            return new TableOrderViewModel
            {
                TableNumber = tableNumber,
                Guests      = guests,
                TotalAmount = subtotal + lowVat + highVat,
                LowVAT      = lowVat,
                HighVAT     = highVat
            };
        }

        public int GetOrderCount(int tableNumber)
        {
            return _tableRepository.GetOrderCount(tableNumber);
        }

        public int GetGuestCount(int tableNumber)
        {
            return _tableRepository.GetGuestCount(tableNumber);
        }

        public List<(int GuestID, string GuestName)> GetGuestsByTable(int tableNumber)
        {
            return _tableRepository.GetGuestsByTable(tableNumber);
        }

        public (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber)
        {
            return _tableRepository.GetRunningOrderCategories(tableNumber);
        }

        public void SetFree(int tableNumber)
        {
            _tableRepository.SetFree(tableNumber);
        }
    }
}