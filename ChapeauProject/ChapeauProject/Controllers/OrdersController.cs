using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    public class OrdersController : ChapeauBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;
        private readonly ITableService _tableService;

        public OrdersController(IOrderService orderService, IMenuService menuService, ITableService tableService)
        {
            _orderService = orderService;
            _menuService = menuService;
            _tableService = tableService;
        }

        public IActionResult Index(string filter = OrderFilter.Running)
        {
            var staff = GetLoggedInStaff();
            try
            {
                var viewModel = new OrdersIndexViewModel
                {
                    TableGroups = _orderService.GetOrdersGroupedByTable(filter, staff.Role),
                    Filter = filter,
                    StaffRole = staff.Role,
                    PageTitle = filter == OrderFilter.Finished ? "Finished Orders Today" : "Running Orders",
                    EmptyMessage = filter == OrderFilter.Finished ? "No finished orders today yet." : "No running orders at the moment."
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[OrdersController.Index] {ex}");
                return View(new OrdersIndexViewModel());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult StartNewOrder(int tableNumber)
        {
            var loggedInStaff = GetLoggedInStaff();
            SetActiveOrder(new Order
            {
                Table = _tableService.GetByTableNumber(tableNumber) ?? new Table { TableNumber = tableNumber },
                Staff = loggedInStaff
            });
            return RedirectToAction("Details", "Tables", new { tableNumber });
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult AddItemToOrder(int menuItemID)
        {
            var order = GetActiveOrder();
            var item = _menuService.GetMenuItemById(menuItemID);
            order.AddItem(item);
            SetActiveOrder(order);
            return RedirectToBasket(order.Table.TableNumber);
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult IncreaseQuantity(int menuItemID)
        {
            var order = GetActiveOrder();
            order.IncreaseQuantity(menuItemID);
            SetActiveOrder(order);
            return RedirectToBasket(order.Table.TableNumber);
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult DecreaseQuantity(int menuItemID)
        {
            var order = GetActiveOrder();
            order.DecreaseQuantity(menuItemID);
            SetActiveOrder(order);
            return RedirectToBasket(order.Table.TableNumber);
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult RemoveRow(int menuItemID)
        {
            var order = GetActiveOrder();
            order.RemoveItem(menuItemID);
            SetActiveOrder(order);
            return RedirectToBasket(order.Table.TableNumber);
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult UpdateItemComment(int menuItemID, string comment)
        {
            var order = GetActiveOrder();
            order.UpdateItemComment(menuItemID, comment);
            SetActiveOrder(order);
            return RedirectToBasket(order.Table.TableNumber);
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult SaveAndSendOrder(Guest guest)
        {
            try
            {
                var order = GetActiveOrder();
                order.Guest = guest;
                _orderService.SaveNewOrder(order);
                ClearActiveOrder();
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[OrdersController.SaveAndSendOrder] {ex}");
                TempData["ErrorMessage"] = "Failed to save order. Please try again.";
                return RedirectToAction("Index", "Tables");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Waiter,Manager")]
        public IActionResult CancelWholeOrder()
        {
            ClearActiveOrder();
            return RedirectToAction("Index", "Tables");
        }

        [HttpPost]
        public IActionResult ToggleCourse(int orderId, CourseName courseName)
        {
            _orderService.ToggleCoursePreparation(orderId, courseName);
            return RedirectToAction("Index");
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
            try
            {
                _orderService.CompleteOrder(orderId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}