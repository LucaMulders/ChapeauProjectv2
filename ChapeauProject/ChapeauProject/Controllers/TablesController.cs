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
                var viewModel = _tableService.GetTableSummaries();

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
                _tableService.ToggleOccupied(tableNumber);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update table: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        //NOTE 'id' matches the default ASP.NET route segment. Renaming to tableNumber means updating 10+ view links and 8 redirects. Should we?
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
                        Table = _tableService.GetByTableNumber(id) ?? new Table { TableNumber = id },
                        Staff = loggedInStaff
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
