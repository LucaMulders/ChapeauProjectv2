using ChapeauProject.Models;

namespace ChapeauProject.Services
{
    public class OrderStateService : IOrderStateService
    {
        public Order ActiveWorkingOrder { get; set; } = new Order();
    }
}
