using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IOrderRepository
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
        void SaveNewOrder(Order order);
    }
}