using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IOrderRepository
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        List<RunningOrderViewModel> GetFinishedOrdersToday();
        void ToggleItemPreparation(int orderItemId);
        void ToggleCoursePreparation(int orderId, string courseName);
        void CompleteOrder(int orderId);
        bool AllItemsReady(int orderId);
        void SaveNewOrder(Order order);
    }
}