using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using System.Collections.Generic;

namespace ChapeauProject.Services
{
    public interface IOrderService
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        List<RunningOrderViewModel> GetFinishedOrdersToday();
        List<TableOrderGroupViewModel> GetOrdersGroupedByTable(string filter, StaffRole role);
        string? ValidateSaveOrder(Order order);
        string? ValidateAddItem(Order order, int menuItemID);
        string? ValidateIncreaseQuantity(Order order, int menuItemID);
        void ToggleItemPreparation(int orderItemId);
        void ToggleCoursePreparation(int orderId, CourseName courseName);
        void CompleteOrder(int orderId);
        void SaveNewOrder(Order order);
    }
}
