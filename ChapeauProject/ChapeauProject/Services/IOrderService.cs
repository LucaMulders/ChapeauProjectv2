using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface IOrderService
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
    }
}