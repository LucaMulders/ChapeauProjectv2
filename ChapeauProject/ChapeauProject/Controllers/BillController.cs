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

        public IActionResult Index()
        {
            var tables = _tableService.GetAllOccupiedTables()
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

        [HttpGet]
        public IActionResult Pay(PayRequest request)
        {
            TableOrderViewModel orders = _tableService.GetTableOrders(request.TableNumber);

            var viewModel = new BillViewModel
            {
                TableNumber    = orders.TableNumber,
                Guests         = orders.Guests,
                SubTotalAmount = orders.SubTotalAmount,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SetSplitMode(int tableNumber, SplitMode splitMode, int splitCount = 1)
        {
            TableOrderViewModel orders = _tableService.GetTableOrders(tableNumber);

            var viewModel = new BillViewModel
            {
                TableNumber    = orders.TableNumber,
                Guests         = orders.Guests,
                SubTotalAmount = orders.SubTotalAmount,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount,
                SplitMode      = splitMode,
            };

            if (splitMode == SplitMode.Equal)
            {
                viewModel.SplitCount = Math.Max(1, splitCount);
            }
            else
            {
                viewModel.SplitCount = 1;
            }

            if (splitMode == SplitMode.Equal)
            {
                decimal share = Math.Round(orders.TotalAmount / viewModel.SplitCount, 2);
                for (int i = 1; i <= viewModel.SplitCount; i++)
                {
                    viewModel.Payers.Add(new SplitPayerViewModel
                    {
                        Name      = $"Person {i}",
                        AmountDue = share
                    });
                }
            }
            else if (splitMode == SplitMode.Custom)
            {
                viewModel.Payers.Add(new SplitPayerViewModel
                {
                    Name      = "Person 1",
                    AmountDue = 0
                });
            }
            else if (splitMode == SplitMode.ByGuest)
            {
                foreach (var guest in orders.Guests)
                {
                    decimal guestTotal = guest.Items.Sum(i => i.Price * i.Quantity);
                    decimal vatShare;
                    if (orders.TotalAmount > 0)
                    {
                        vatShare = guestTotal * (orders.TotalAmount / orders.SubTotalAmount);
                    }
                    else
                    {
                        vatShare = guestTotal;
                    }

                    viewModel.Payers.Add(new SplitPayerViewModel
                    {
                        Name      = guest.GuestName,
                        GuestID   = guest.GuestID,
                        AmountDue = Math.Round(vatShare, 2)
                    });
                }
            }

            return View("Pay", viewModel);
        }

        [HttpPost]
        public IActionResult Pay(BillViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Custom mode: validate sum of payments >= total
            if (model.SplitMode == SplitMode.Custom)
            {
                decimal totalPaying = model.Payers.Sum(p => p.AmountDue + p.TipAmount);
                if (totalPaying < model.TotalAmount)
                {
                    ModelState.AddModelError("", $"Total payments (€{totalPaying:F2}) are less than the bill total (€{model.TotalAmount:F2}).");
                    // Reload guest data for the view
                    var orders = _tableService.GetTableOrders(model.TableNumber);
                    model.Guests = orders.Guests;
                    return View(model);
                }
            }

            _billService.ProcessPayment(model);

            return RedirectToAction("Confirmation", new { id = model.TableNumber });
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            ViewBag.TableNumber = id;
            return View();
        }
    }
}
