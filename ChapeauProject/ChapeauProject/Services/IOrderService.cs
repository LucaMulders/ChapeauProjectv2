using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public interface IOrderService
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
        bool AllItemsReady(int orderId);
        void SaveNewOrder(Order order);
    }
}