using ChapeauProject.Models;
using ChapeauProject.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChapeauProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;

        private static Order _activeWorkingOrder = new Order();

        
        public static Order ActiveWorkingOrder
        {
            get => _activeWorkingOrder;
            set => _activeWorkingOrder = value;
        }

        public OrdersController(IOrderService orderService, IMenuService menuService)
        {
            _orderService = orderService;
            _menuService = menuService;
        }

        public IActionResult Index(string filter = "running")
        {
            var orders = filter == "finished"
                ? _orderService.GetFinishedOrdersToday()
                : _orderService.GetAllOrdersByStatus();

            ViewBag.Filter = filter;
            return View(orders);
        }

        [HttpPost]
        public IActionResult StartNewOrder(int tableNumber)
        {
            _activeWorkingOrder = new Order
            {
                TableNumber = tableNumber,
                OrderItems = new System.Collections.Generic.List<OrderItem>()
            };

            return RedirectToAction("Details", "Tables", new { id = tableNumber });
        }

        [HttpPost]
        public IActionResult AddItemToOrder(int menuItemID)
        {
            var item = _menuService.GetById(menuItemID);
            if (item == null)
                return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });

            if (item.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = $"{item.ItemName} is out of stock!";
                return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
            }

            var existingItem = _activeWorkingOrder.OrderItems
                .FirstOrDefault(oi => oi.MenuItemID == menuItemID);

            if (existingItem != null)
            {
                if (existingItem.Quantity >= item.StockQuantity)
                {
                    TempData["ErrorMessage"] = $"Stock ceiling reached for {item.ItemName}.";
                    return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
                }
                existingItem.Quantity++;
            }
            else
            {
                _activeWorkingOrder.OrderItems.Add(new OrderItem
                {
                    MenuItemID = menuItemID,
                    MenuItem = item,
                    Quantity = 1,
                    PreparationStatus = PreparationStatus.Pending,
                    Comment = string.Empty
                });
            }

            return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int menuItemID)
        {
            var basketItem = _activeWorkingOrder.OrderItems.FirstOrDefault(oi => oi.MenuItemID == menuItemID);
            var dbItem = _menuService.GetById(menuItemID);

            if (basketItem != null && dbItem != null)
            {
                if (basketItem.Quantity >= dbItem.StockQuantity)
                {
                    TempData["ErrorMessage"] = "Cannot exceed warehouse stock capacities.";
                }
                else
                {
                    basketItem.Quantity++;
                }
            }

            return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int menuItemID)
        {
            var basketItem = _activeWorkingOrder.OrderItems.FirstOrDefault(oi => oi.MenuItemID == menuItemID);
            if (basketItem != null)
            {
                basketItem.Quantity--;
                if (basketItem.Quantity <= 0)
                {
                    _activeWorkingOrder.OrderItems.Remove(basketItem);
                }
            }
            
            return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
        }

        [HttpPost]
        public IActionResult RemoveRow(int menuItemID)
        {
            var basketItem = _activeWorkingOrder.OrderItems.FirstOrDefault(oi => oi.MenuItemID == menuItemID);
            if (basketItem != null)
            {
                _activeWorkingOrder.OrderItems.Remove(basketItem);
            }
           
            return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
        }

        [HttpPost]
        public IActionResult UpdateItemComment(int menuItemID, string comment)
        {
            var basketItem = _activeWorkingOrder.OrderItems.FirstOrDefault(oi => oi.MenuItemID == menuItemID);
            if (basketItem != null)
            {
                basketItem.Comment = comment ?? string.Empty;
            }
       
            return RedirectToAction("Details", "Tables", new { id = _activeWorkingOrder.TableNumber });
        }

        [HttpPost]
        public IActionResult SaveAndSendOrder(int guestId)
        {
            int currentTableId = _activeWorkingOrder.TableNumber;

            if (guestId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a guest before sending the order.";
                return RedirectToAction("Details", "Tables", new { id = currentTableId });
            }

            if (!_activeWorkingOrder.OrderItems.Any())
            {
                TempData["ErrorMessage"] = "The active order sheet cannot be blank.";
                return RedirectToAction("Details", "Tables", new { id = currentTableId });
            }

            _activeWorkingOrder.GuestID = guestId;
            _orderService.SaveNewOrder(_activeWorkingOrder);

            TempData["SuccessMessage"] = "Order dispatched and stock adjusted successfully!";
            _activeWorkingOrder = new Order();

           
            return RedirectToAction("Index", "Tables");
        }

        [HttpPost]
        public IActionResult CancelWholeOrder()
        {
            _activeWorkingOrder = new Order();
            TempData["InfoMessage"] = "Order sheet reset.";
            return RedirectToAction("Index", "Tables");
        }

        [HttpPost]
        public IActionResult ToggleCourse(int orderId, string courseName)
        {
            _orderService.ToggleCoursePreparation(orderId, courseName);
            return RedirectToAction("Index");
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
            if (!_orderService.AllItemsReady(orderId))
            {
                TempData["ErrorMessage"] = "All items must be Ready before marking the order as served.";
                return RedirectToAction("Index");
            }

            _orderService.CompleteOrder(orderId);
            return RedirectToAction("Index");
        }
    }
}