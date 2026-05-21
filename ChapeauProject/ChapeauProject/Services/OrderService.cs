using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<RunningOrderViewModel> GetRunningOrders()
        {
            return _orderRepository.GetRunningOrders();
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            _orderRepository.ToggleItemPreparation(orderItemId);
        }

        public void CompleteOrder(int orderId)
        {
            _orderRepository.CompleteOrder(orderId);
        }
    }
}