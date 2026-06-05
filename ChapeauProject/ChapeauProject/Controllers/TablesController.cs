using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChapeauProject.Controllers
{
    //NOTE: No exception handling in this controller — rubric requires exceptions are handled and should not crash the application
    public class TablesController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IMenuService _menuService;
        private readonly IOrderStateService _orderState;

        public TablesController(ITableService tableService, IMenuService menuService, IOrderStateService orderState)
        {
            _tableService = tableService;
            _menuService = menuService;
            _orderState = orderState;
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
            // existing running orders view model for the top of the screen
            var viewModel = _tableService.GetTableOrders(id);
            if (viewModel == null) return NotFound();

          
            if (_orderState.ActiveWorkingOrder.Table.TableNumber != id)
            {
                _orderState.ActiveWorkingOrder = new Order
                {
                    Table = _tableService.GetByTableNumber(id) ?? new Table { TableNumber = id },
                    OrderItems = new System.Collections.Generic.List<OrderItem>()
                };
            }

            ViewBag.CardFilter = cardFilter;
            ViewBag.CourseFilter = courseFilter;
            ViewBag.CurrentBasket = _orderState.ActiveWorkingOrder;
            ViewBag.Guests = _tableService.GetGuestsByTable(id);
            ViewBag.MenuItems = _menuService.GetCourseFiltered(cardFilter, courseFilter);

         
            return View(viewModel);
        }
    }
}