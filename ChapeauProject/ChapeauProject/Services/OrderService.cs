using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuService _menuService;

        public OrderService(IOrderRepository orderRepository, IMenuService menuService)
        {
            _orderRepository = orderRepository;
            _menuService = menuService;
        }

        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            return MapToViewModels(_orderRepository.GetAllOrdersByStatus());
        }

        public List<RunningOrderViewModel> GetFinishedOrdersToday()
        {
            return MapToViewModels(_orderRepository.GetFinishedOrdersToday());
        }

        public List<TableOrderGroupViewModel> GetOrdersGroupedByTable(string filter, StaffRole role)
        {
            List<RunningOrderViewModel> orders = filter == OrderFilter.Finished
                ? GetFinishedOrdersToday()
                : GetAllOrdersByStatus();

            // Filter by role: Chef sees food items, Bartender sees drink items
            List<RunningOrderViewModel> filtered = new List<RunningOrderViewModel>();
            foreach (var order in orders)
            {
                bool include = false;

                if (role == StaffRole.Chef)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.MenuCard == MenuCard.Lunch || item.MenuCard == MenuCard.Dinner)
                        {
                            include = true;
                            break;
                        }
                    }
                }
                else if (role == StaffRole.Bartender)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.MenuCard == MenuCard.Drinks)
                        {
                            include = true;
                            break;
                        }
                    }
                }
                else
                {
                    include = true;
                }

                if (include)
                    filtered.Add(order);
            }

            // Group by table number
            Dictionary<int, List<RunningOrderViewModel>> grouped = new Dictionary<int, List<RunningOrderViewModel>>();
            foreach (var order in filtered)
            {
                if (!grouped.ContainsKey(order.TableNumber))
                    grouped[order.TableNumber] = new List<RunningOrderViewModel>();
                grouped[order.TableNumber].Add(order);
            }

            // Sort by table number and convert to result
            List<int> tableNumbers = new List<int>();
            foreach (var key in grouped.Keys)
                tableNumbers.Add(key);

            for (int i = 0; i < tableNumbers.Count - 1; i++)
            {
                for (int j = i + 1; j < tableNumbers.Count; j++)
                {
                    if (tableNumbers[j] < tableNumbers[i])
                    {
                        int temp = tableNumbers[i];
                        tableNumbers[i] = tableNumbers[j];
                        tableNumbers[j] = temp;
                    }
                }
            }

            List<TableOrderGroupViewModel> result = new List<TableOrderGroupViewModel>();
            foreach (var tableNumber in tableNumbers)
            {
                result.Add(new TableOrderGroupViewModel
                {
                    TableNumber = tableNumber,
                    Orders = grouped[tableNumber]
                });
            }

            return result;
        }

        public string? ValidateSaveOrder(Order order)
        {
            if (order.Guest.GuestID <= 0)
                return "Please select a guest before sending the order.";

            if (order.OrderItems.Count == 0)
                return "The active order sheet cannot be blank.";

            return null;
        }

        public string? ValidateAddItem(Order order, int menuItemID)
        {
            var item = _menuService.GetMenuItemById(menuItemID);
            if (item == null)
                return "Item not found.";
            if (!item.IsAvailable)
                return $"{item.ItemName} is out of stock!";

            OrderItem existing = null;
            foreach (var oi in order.OrderItems)
            {
                if (oi.MenuItem?.MenuItemID == menuItemID)
                {
                    existing = oi;
                    break;
                }
            }

            if (existing != null && !item.IsInStock(existing.Quantity))
                return $"Stock ceiling reached for {item.ItemName}.";

            return null;
        }

        public string? ValidateIncreaseQuantity(Order order, int menuItemID)
        {
            var dbItem = _menuService.GetMenuItemById(menuItemID);

            OrderItem basketItem = null;
            foreach (var oi in order.OrderItems)
            {
                if (oi.MenuItem?.MenuItemID == menuItemID)
                {
                    basketItem = oi;
                    break;
                }
            }

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

        private List<RunningOrderViewModel> MapToViewModels(List<Order> orders)
        {
            List<RunningOrderViewModel> result = new List<RunningOrderViewModel>();

            foreach (var order in orders)
            {
                List<RunningOrderItemViewModel> items = new List<RunningOrderItemViewModel>();

                foreach (var orderItem in order.OrderItems)
                {
                    items.Add(new RunningOrderItemViewModel
                    {
                        OrderItemID = orderItem.OrderItemID,
                        ItemName = orderItem.MenuItem?.ItemName ?? string.Empty,
                        Quantity = orderItem.Quantity,
                        PreparationStatus = orderItem.PreparationStatus,
                        MenuCard = orderItem.MenuItem?.Card ?? default,
                        CourseName = orderItem.CourseName ?? CourseName.Other,
                        Comment = orderItem.Comment
                    });
                }

                result.Add(new RunningOrderViewModel
                {
                    OrderID = order.OrderID,
                    TableNumber = order.Table?.TableNumber ?? 0,
                    OrderTime = order.OrderTimeStamp,
                    Items = items
                });
            }

            return result;
        }
    }
}