using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface IOrderService
    {
        List<RunningOrderViewModel> GetRunningOrders();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
    }
}