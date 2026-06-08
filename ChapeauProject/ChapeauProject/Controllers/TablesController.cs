using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChapeauProject.Controllers
{
    public class TablesController : ChapeauBaseController
    {
        private readonly ITableService _tableService;
        private readonly IMenuService _menuService;

        public TablesController(ITableService tableService, IMenuService menuService)
        {
            _tableService = tableService;
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            try
            {
                var tables = _tableService.GetAllTables();
                var viewModel = tables.Select(t =>
                {
                    var categories = _tableService.GetRunningOrderCategories(t.TableNumber);
                    int guestCount;
                    if (t.IsOccupied)
                        guestCount = _tableService.GetGuestCount(t.TableNumber);
                    else
                        guestCount = 0;

                    var vm = new TablesViewModel
                    {
                        Table         = t,
                        OrderCount    = _tableService.GetOrderCount(t.TableNumber),
                        HasFoodOrder  = categories.HasFood,
                        HasDrinkOrder = categories.HasDrink,
                        GuestCount    = guestCount
                    };
                    return vm;
                }).OrderBy(t => t.Table.TableNumber).ToList();

                ViewBag.FreeCount     = viewModel.Count(t => !t.Table.IsOccupied);
                ViewBag.OccupiedCount = viewModel.Count(t =>  t.Table.IsOccupied);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load tables: " + ex.Message;
                return View(new List<TablesViewModel>());
            }
        }

        [HttpPost]
        public IActionResult ToggleOccupied(int tableNumber)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update table: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id, MenuCard cardFilter = MenuCard.Lunch, string courseFilter = CourseFilter.All)
        {
            try
            {
                var viewModel = _tableService.GetTableOrders(id);
                if (viewModel == null) return NotFound();

                var activeOrder = GetActiveOrder();
                if (activeOrder.Table.TableNumber != id)
                {
                    var loggedInStaff = GetLoggedInStaff();
                    activeOrder = new Order
                    {
                        Table      = _tableService.GetByTableNumber(id) ?? new Table { TableNumber = id },
                        Staff      = loggedInStaff,
                        OrderItems = new System.Collections.Generic.List<OrderItem>()
                    };
                    SetActiveOrder(activeOrder);
                }

                ViewBag.CardFilter    = cardFilter;
                ViewBag.CourseFilter  = courseFilter;
                ViewBag.CurrentBasket = activeOrder;
                ViewBag.Guests        = _tableService.GetGuestsByTable(id);
                ViewBag.MenuItems     = _menuService.GetCourseFiltered(cardFilter, courseFilter);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load table details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
