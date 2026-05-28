using ChapeauProject.ViewModels;

namespace ChapeauProject.Repositories
{
    public interface IOrderRepository
    {
        List<RunningOrderViewModel> GetAllOrdersByStatus();
        void ToggleItemPreparation(int orderItemId);
        void CompleteOrder(int orderId);
    }
}