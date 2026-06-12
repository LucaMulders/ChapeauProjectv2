using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuService     _menuService;

        public OrderService(IOrderRepository orderRepository, IMenuService menuService)
        {
            _orderRepository = orderRepository;
            _menuService     = menuService;
        }

        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            return MapToViewModels(_orderRepository.GetAllOrdersByStatus());
        }

        public List<RunningOrderViewModel> GetFinishedOrdersToday()
        {
            return MapToViewModels(_orderRepository.GetFinishedOrdersToday());
        }

        public List<TableOrderGroupViewModel> GetOrdersGroupedByTable(string filter)
        {
            List<RunningOrderViewModel> orders;
            if (filter == OrderFilter.Finished)
                orders = GetFinishedOrdersToday();
            else
                orders = GetAllOrdersByStatus();

            return orders
                .GroupBy(o => o.TableNumber)
                .OrderBy(g => g.Key)
                .Select(g => new TableOrderGroupViewModel
                {
                    TableNumber = g.Key,
                    Orders      = g.ToList()
                })
                .ToList();
        }

        //NOTE use orders guest
        public string? ValidateSaveOrder(Order order, Guest guest)
        {
            if (guest.GuestID <= 0)
                return "Please select a guest before sending the order.";

            if (!order.OrderItems.Any())
                return "The active order sheet cannot be blank.";

            return null;
        }

        public string? ValidateAddItem(Order order, int menuItemID)
        {
            var item = _menuService.GetMenuItemById(menuItemID);
            if (item == null) return "Item not found.";
            if (!item.IsAvailable) return $"{item.ItemName} is out of stock!";

            var existing = order.OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (existing != null && !item.IsInStock(existing.Quantity))
                return $"Stock ceiling reached for {item.ItemName}.";

            return null;
        }

        public string? ValidateIncreaseQuantity(Order order, int menuItemID)
        {
            var dbItem     = _menuService.GetMenuItemById(menuItemID);
            var basketItem = order.OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);

            if (basketItem != null && dbItem != null && !dbItem.IsInStock(basketItem.Quantity))
                return "Cannot exceed warehouse stock capacities.";

            return null;
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            _orderRepository.ToggleItemPreparation(orderItemId);
        }

        public void ToggleCoursePreparation(int orderId, CourseName courseName)
        {
            _orderRepository.ToggleCoursePreparation(orderId, courseName);
        }

        public void CompleteOrder(int orderId)
        {
            if (!_orderRepository.AllItemsReady(orderId))
                throw new InvalidOperationException("All items must be Served before marking the order as complete.");

            _orderRepository.CompleteOrder(orderId);
        }

        public void SaveNewOrder(Order order)
        {
            _orderRepository.SaveNewOrder(order);

            foreach (var item in order.OrderItems)
            {
                if (item.MenuItem != null)
                    _menuService.DeductStockQuantity(item.MenuItem.MenuItemID, item.Quantity);
            }
        }

        private List<RunningOrderViewModel> MapToViewModels(List<RunningOrder> orders)
        {
            return orders.Select(o => new RunningOrderViewModel
            {
                OrderID     = o.OrderID,
                TableNumber = o.TableNumber,
                OrderTime   = o.OrderTime,
                Items       = o.Items.Select(i => new RunningOrderItemViewModel
                {
                    OrderItemID       = i.OrderItemID,
                    ItemName          = i.MenuItem.ItemName,
                    Quantity          = i.Quantity,
                    PreparationStatus = i.PreparationStatus,
                    MenuCard          = i.MenuItem.AssociatedMenu?.CardType.ToString() ?? string.Empty,
                    CourseName        = i.CourseName,
                    Comment           = i.Comment
                }).ToList()
            }).ToList();
        }
    }
}
