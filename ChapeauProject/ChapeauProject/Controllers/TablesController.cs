using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChapeauProject.Controllers
{
    [Authorize(Roles = "Waiter,Manager")]
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
                var viewModel = _tableService.GetTableSummaries();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Changed errors to be more generic to avoid giving away information about the system

                Console.Error.WriteLine($"[TablesController.Index] {ex}");
                TempData["ErrorMessage"] = "Failed to load tables. Please try again.";
                return View(new List<TablesViewModel>());
            }
        }

        [HttpPost]
        public IActionResult ToggleOccupied(int tableNumber)
        {
            try
            {
                _tableService.ToggleOccupied(tableNumber);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TablesController.ToggleOccupied] {ex}");
                TempData["ErrorMessage"] = "Failed to update table. Please try again.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Details(int tableNumber, MenuCard cardFilter = MenuCard.Lunch, string courseFilter = CourseFilter.All)
        {
            try
            {
                var table = _tableService.GetByTableNumber(tableNumber);
                if (table == null) return NotFound();

                var viewModel = _tableService.GetTableOrders(tableNumber);
                if (viewModel == null) return NotFound();

                var activeOrder = GetActiveOrder();
                if (activeOrder.Table.TableNumber != tableNumber)
                {
                    var loggedInStaff = GetLoggedInStaff();
                    activeOrder = new Order
                    {
                        Table = table,
                        Staff = loggedInStaff
                    };
                    SetActiveOrder(activeOrder);
                }

                var workspaceViewModel = new TableWorkspaceViewModel
                {
                    TableOrders  = viewModel,
                    ActiveBasket = activeOrder,
                    Guests       = _tableService.GetGuestsByTable(tableNumber),
                    MenuItems    = _menuService.GetCourseFiltered(cardFilter, courseFilter),
                    CardFilter   = cardFilter,
                    CourseFilter = courseFilter
                };

                return View(workspaceViewModel);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TablesController.Details] {ex}");
                TempData["ErrorMessage"] = "Failed to load table details. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}
