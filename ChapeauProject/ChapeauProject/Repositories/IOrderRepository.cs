using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IOrderRepository
    {
        List<Order> GetAllOrdersByStatus();
        List<Order> GetFinishedOrdersToday();
        void ToggleItemPreparation(int orderItemId);
        void ToggleCoursePreparation(int orderId, CourseName courseName);
        void CompleteOrder(int orderId);
        bool AllItemsReady(int orderId);
        void SaveNewOrder(Order order);
    }
}