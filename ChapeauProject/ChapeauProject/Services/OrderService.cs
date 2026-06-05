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
            var orders = filter == "finished"
                ? GetFinishedOrdersToday()
                : GetAllOrdersByStatus();

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

        public string? ValidateAddItem(Order order, int menuItemID)
        {
            var item = _menuService.GetById(menuItemID);
            if (item == null) return "Item not found.";
            if (!item.IsAvailable) return $"{item.ItemName} is out of stock!";

            var existing = order.OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);
            if (existing != null && !item.IsInStock(existing.Quantity))
                return $"Stock ceiling reached for {item.ItemName}.";

            return null;
        }

        public string? ValidateIncreaseQuantity(Order order, int menuItemID)
        {
            var dbItem     = _menuService.GetById(menuItemID);
            var basketItem = order.OrderItems.FirstOrDefault(oi => oi.MenuItem?.MenuItemID == menuItemID);

            if (basketItem != null && dbItem != null && !dbItem.IsInStock(basketItem.Quantity))
                return "Cannot exceed warehouse stock capacities.";

            return null;
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            _orderRepository.ToggleItemPreparation(orderItemId);
        }

        public void ToggleCoursePreparation(int orderId, string courseName)
        {
            _orderRepository.ToggleCoursePreparation(orderId, courseName);
        }

        public void CompleteOrder(int orderId)
        {
            _orderRepository.CompleteOrder(orderId);
        }

        public bool AllItemsReady(int orderId)
        {
            return _orderRepository.AllItemsReady(orderId);
        }

        public void SaveNewOrder(Order order)
        {
            _orderRepository.SaveNewOrder(order);
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
                    ItemName          = i.ItemName,
                    Quantity          = i.Quantity,
                    PreparationStatus = i.PreparationStatus,
                    MenuCard          = i.MenuCard,
                    CourseName        = i.CourseName,
                    Comment           = i.Comment
                }).ToList()
            }).ToList();
        }
    }
}
