using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    public class BillController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IBillService  _billService;

        public BillController(ITableService tableService, IBillService billService)
        {
            _tableService = tableService;
            _billService  = billService;
        }

        // Lists all occupied tables that have pending orders.
        public IActionResult Index()
        {
            var tables = _tableService.GetAllTables()
                .Where(t => t.IsOccupied)
                .Select(t =>
                {
                    var categories = _tableService.GetRunningOrderCategories(t.TableNumber);
                    return new TablesViewModel
                    {
                        Table         = t,
                        OrderCount    = _tableService.GetOrderCount(t.TableNumber),
                        HasFoodOrder  = categories.HasFood,
                        HasDrinkOrder = categories.HasDrink
                    };
                })
                .ToList();

            return View(tables);
        }

        // Shows the bill + payment form for a specific table.
        [HttpGet]
        public IActionResult Pay(int id)
        {
            var orders = _tableService.GetTableOrders(id);

            var viewModel = new BillViewModel
            {
                TableNumber    = id,
                SubTotalAmount = orders.TotalAmount - orders.LowVAT - orders.HighVAT,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount
            };

            return View(viewModel);
        }

        // Processes the payment and frees the table.
        [HttpPost]
        public IActionResult Pay(BillViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _billService.ProcessPayment(model);

            return RedirectToAction("Confirmation", new { id = model.TableNumber });
        }

        // Shown after a successful payment.
        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            ViewBag.TableNumber = id;
            return View();
        }
    }
}
