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
            var orders = _orderService.GetAllOrdersByStatus();
            return View(orders);
        }

        [HttpPost]
        public IActionResult ToggleItem(int orderItemId, string? returnUrl)
        {
            _orderService.ToggleItemPreparation(orderItemId);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
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