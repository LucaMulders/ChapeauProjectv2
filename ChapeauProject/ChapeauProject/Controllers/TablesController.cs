using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChapeauProject.Controllers
{
    //NOTE No exception handling in this controller, needs to be added
    public class TablesController : Controller
    {
        private const string SessionKey = "ActiveWorkingOrder";

        private readonly ITableService _tableService;
        private readonly IMenuService _menuService;

        public TablesController(ITableService tableService, IMenuService menuService)
        {
            _tableService = tableService;
            _menuService = menuService;
        }

        private Order GetActiveOrder()
        {
            return HttpContext.Session.GetObject<Order>(SessionKey) ?? new Order();
        }

        private void SetActiveOrder(Order order)
        {
            HttpContext.Session.SetObject(SessionKey, order);
        }

        public IActionResult Index()
        {
            var tables = _tableService.GetAllTables();
            var viewModel = tables.Select(t =>
            {
                var categories = _tableService.GetRunningOrderCategories(t.TableNumber);
                var vm = new TablesViewModel
                {
                    Table = t,
                    OrderCount = _tableService.GetOrderCount(t.TableNumber),
                    HasFoodOrder = categories.HasFood,
                    HasDrinkOrder = categories.HasDrink
                };
                if (t.IsOccupied)
                {
                    vm.GuestCount = _tableService.GetGuestCount(t.TableNumber);
                }
                else
                {
                    vm.GuestCount = 0;
                }
                return vm;
            }).OrderBy(t => t.Table.TableNumber).ToList();

            ViewBag.FreeCount     = viewModel.Count(t => !t.Table.IsOccupied);
            ViewBag.OccupiedCount = viewModel.Count(t =>  t.Table.IsOccupied);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ToggleOccupied(int tableNumber)
        {
            var table = _tableService.GetByTableNumber(tableNumber);
            if (table != null && table.IsOccupied)
            {
                int pendingOrders = _tableService.GetOrderCount(tableNumber);
                if (pendingOrders > 0)
                {
                    TempData["Error"] = $"Table {tableNumber} still has {pendingOrders} pending order(s) and cannot be marked as free.";
                    return RedirectToAction("Index");
                }
            }

            _tableService.ToggleOccupied(tableNumber);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id, MenuCard cardFilter = MenuCard.Lunch, string courseFilter = "All")
        {
            var viewModel = _tableService.GetTableOrders(id);
            if (viewModel == null) return NotFound();

            var activeOrder = GetActiveOrder();
            if (activeOrder.Table.TableNumber != id)
            {
                activeOrder = new Order
                {
                    Table = _tableService.GetByTableNumber(id) ?? new Table { TableNumber = id },
                    OrderItems = new System.Collections.Generic.List<OrderItem>()
                };
                SetActiveOrder(activeOrder);
            }

            ViewBag.CardFilter = cardFilter;
            ViewBag.CourseFilter = courseFilter;
            ViewBag.CurrentBasket = activeOrder;
            ViewBag.Guests = _tableService.GetGuestsByTable(id);
            ViewBag.MenuItems = _menuService.GetCourseFiltered(cardFilter, courseFilter);

            return View(viewModel);
        }
    }
}
