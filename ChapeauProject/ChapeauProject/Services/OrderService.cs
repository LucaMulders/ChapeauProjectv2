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
            return _orderRepository.GetAllOrdersByStatus();
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            _orderRepository.ToggleItemPreparation(orderItemId);
        }

        public void CompleteOrder(int orderId)
        {
            _orderRepository.CompleteOrder(orderId);
        }

        public bool AllItemsReady(int orderId)
        {
            return _orderRepository.AllItemsReady(orderId);
        }

        // FIXED: Added missing implementation to fulfill your interface contract requirement
        public void SaveNewOrder(Order order)
        {
            _orderRepository.SaveNewOrder(order);
        }
    }
}