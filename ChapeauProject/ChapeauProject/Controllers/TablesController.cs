using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    public class TablesController : Controller
    {
        private readonly ITableService _tableService;

        public TablesController(ITableService tableService)
        {
            _tableService = tableService;
        }

        public IActionResult Index()
        {
            var tables = _tableService.GetAll();
            var viewModel = tables.Select(t => new TablesViewModel
            {
                Table = t,
                OrderCount = _tableService.GetOrderCount(t.TableNumber)
            }).ToList();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ToggleOccupied(int tableNumber)
        {
            _tableService.ToggleOccupied(tableNumber);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var viewModel = _tableService.GetTableOrders(id);
            return View(viewModel);
        }
    }
}