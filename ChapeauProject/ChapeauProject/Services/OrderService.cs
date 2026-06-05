using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            return MapToViewModels(_orderRepository.GetAllOrdersByStatus());
        }

        public List<RunningOrderViewModel> GetFinishedOrdersToday()
        {
            return MapToViewModels(_orderRepository.GetFinishedOrdersToday());
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
    }
}
