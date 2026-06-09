using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public class TableService : ITableService
    {
        private const decimal LowVatRate  = 0.09m;
        private const decimal HighVatRate = 0.21m;

        private readonly ITableRepository _tableRepository;

        public TableService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public List<Table> GetAllTables()
        {
            return _tableRepository.GetAllTables();
        }

        public List<TablesViewModel> GetTableSummaries(bool occupiedOnly = false)
        {
            List<Table> tables;
            if (occupiedOnly)
                tables = GetAllOccupiedTables();
            else
                tables = GetAllTables();

            return tables.Select(t =>
            {
                var categories = _tableRepository.GetRunningOrderCategories(t.TableNumber);

                int guestCount;
                if (t.IsOccupied)
                    guestCount = _tableRepository.GetGuestCount(t.TableNumber);
                else
                    guestCount = 0;

                return new TablesViewModel
                {
                    Table         = t,
                    OrderCount    = _tableRepository.GetOrderCount(t.TableNumber),
                    HasFoodOrder  = categories.HasFood,
                    HasDrinkOrder = categories.HasDrink,
                    GuestCount    = guestCount
                };
            }).OrderBy(t => t.Table.TableNumber).ToList();
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
            var table = _tableRepository.GetByTableNumber(tableNumber);
            if (table != null && table.IsOccupied)
            {
                int pendingOrders = _tableRepository.GetOrderCount(tableNumber);
                if (pendingOrders > 0)
                    throw new InvalidOperationException($"Table {tableNumber} still has {pendingOrders} pending order(s) and cannot be marked as free.");
            }

            _tableRepository.ToggleOccupied(tableNumber);
        }

        public TableOrderViewModel GetTableOrders(int tableNumber)
        {
            var guestOrders = _tableRepository.GetTableOrders(tableNumber);

            var guests = guestOrders.Select(g => new GuestOrderViewModel
            {
                GuestID   = g.GuestID,
                FullName  = g.GuestName,
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
            decimal lowVat   = allItems.Where(i => i.VatRate == LowVatRate).Sum(i => i.Price * i.Quantity * i.VatRate);
            decimal highVat  = allItems.Where(i => i.VatRate == HighVatRate).Sum(i => i.Price * i.Quantity * i.VatRate);
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

        public List<Guest> GetGuestsByTable(int tableNumber)
        {
            return _tableRepository.GetGuestsByTable(tableNumber);
        }

        public (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber)
        {
            return _tableRepository.GetRunningOrderCategories(tableNumber);
        }

        public void MarkTableAsFree(int tableNumber)
        {
            _tableRepository.MarkTableAsFree(tableNumber);
        }

        public void RemoveGuests(int tableNumber)
        {
            _tableRepository.RemoveGuests(tableNumber);
        }
    }
}