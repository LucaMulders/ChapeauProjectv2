using ChapeauProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            var orders = _orderService.GetRunningOrders();
            return View(orders);
        }

        [HttpPost]
        public IActionResult ToggleItem(int orderItemId)
        {
            _orderService.ToggleItemPreparation(orderItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CompleteOrder(int orderId)
        {
            _orderService.CompleteOrder(orderId);
            return RedirectToAction("Index");
        }
    }
}