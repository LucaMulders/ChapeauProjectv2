using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    public class OrdersController : ChapeauBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService  _menuService;
        private readonly ITableService _tableService;

        public OrdersController(IOrderService orderService, IMenuService menuService, ITableService tableService)
        {
            _orderService = orderService;
            _menuService  = menuService;
            _tableService = tableService;
        }

        public IActionResult Index(string filter = "running")
        {
            try
            {
                var tableGroups = _orderService.GetOrdersGroupedByTable(filter);
                ViewBag.Filter = filter;

                if (filter == "finished")
                {
                    ViewBag.PageTitle    = "Finished Orders Today";
                    ViewBag.EmptyMessage = "No finished orders today yet.";
                }
                else
                {
                    ViewBag.PageTitle    = "Running Orders";
                    ViewBag.EmptyMessage = "No running orders at the moment.";
                }
                return View(tableGroups);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load orders: " + ex.Message;
                return View(new List<TableOrderGroupViewModel>());
            }
        }

        [HttpPost]
        public IActionResult StartNewOrder(int tableNumber)
        {
            var loggedInStaff = GetLoggedInStaff();
            SetActiveOrder(new Order
            {
                Table      = _tableService.GetByTableNumber(tableNumber) ?? new Table { TableNumber = tableNumber },
                Staff      = loggedInStaff,
                OrderItems = new System.Collections.Generic.List<OrderItem>()
            });
            return RedirectToAction("Details", "Tables", new { id = tableNumber });
        }

        // I added fragment baskets which will makes it so we don't have to scroll down after every basket addition.

        [HttpPost]
        public IActionResult AddItemToOrder(int menuItemID)
        {
            var order = GetActiveOrder();
            string? error = _orderService.ValidateAddItem(order, menuItemID);

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction("Details", "Tables", new { id = order.Table.TableNumber, fragment = "basket" });
            }

            var item = _menuService.GetMenuItemById(menuItemID);
            order.AddItem(item!);
            SetActiveOrder(order);
            return Redirect(Url.Action("Details", "Tables", new { id = order.Table.TableNumber }) + "#basket");
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int menuItemID)
        {
            var order  = GetActiveOrder();
            string? error = _orderService.ValidateIncreaseQuantity(order, menuItemID);

            if (error != null)
                TempData["ErrorMessage"] = error;
            else
                order.IncreaseQuantity(menuItemID);

            SetActiveOrder(order);
            return Redirect(Url.Action("Details", "Tables", new { id = order.Table.TableNumber }) + "#basket");
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int menuItemID)
        {
            var order = GetActiveOrder();
            order.DecreaseQuantity(menuItemID);
            SetActiveOrder(order);
            return Redirect(Url.Action("Details", "Tables", new { id = order.Table.TableNumber }) + "#basket");
        }

        [HttpPost]
        public IActionResult RemoveRow(int menuItemID)
        {
            var order = GetActiveOrder();
            order.RemoveItem(menuItemID);
            SetActiveOrder(order);
            return Redirect(Url.Action("Details", "Tables", new { id = order.Table.TableNumber }) + "#basket");
        }

        [HttpPost]
        public IActionResult UpdateItemComment(int menuItemID, string comment)
        {
            var order = GetActiveOrder();
            order.UpdateItemComment(menuItemID, comment);
            SetActiveOrder(order);
            return Redirect(Url.Action("Details", "Tables", new { id = order.Table.TableNumber }) + "#basket");
        }

        [HttpPost]
        public IActionResult SaveAndSendOrder(Guest guest)
        {
            var order = GetActiveOrder();
            int currentTableId = order.Table.TableNumber;

            if (guest.GuestID <= 0)
            {
                TempData["ErrorMessage"] = "Please select a guest before sending the order.";
                return RedirectToAction("Details", "Tables", new { id = currentTableId });
            }

            if (!order.OrderItems.Any())
            {
                TempData["ErrorMessage"] = "The active order sheet cannot be blank.";
                return RedirectToAction("Details", "Tables", new { id = currentTableId });
            }

            order.Guest = guest;
            _orderService.SaveNewOrder(order);
            TempData["SuccessMessage"] = "Order dispatched and stock adjusted successfully!";
            ClearActiveOrder();
            return RedirectToAction("Index", "Tables");
        }

        [HttpPost]
        public IActionResult CancelWholeOrder()
        {
            ClearActiveOrder();
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
                TempData["ErrorMessage"] = "All items must be Served before marking the order as complete.";
                return RedirectToAction("Index");
            }

            _orderService.CompleteOrder(orderId);
            return RedirectToAction("Index");
        }
    }
}
