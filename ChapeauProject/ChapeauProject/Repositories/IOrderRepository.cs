using ChapeauProject.ViewModels;

namespace ChapeauProject.Repositories
{
    public interface IOrderRepository
    {
        List<RunningOrderViewModel> GetRunningOrders();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
    }
}