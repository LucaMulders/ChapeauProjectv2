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
            try
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load bill overview: " + ex.Message;
                return View(new List<TablesViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Pay(PayRequest request)
        {
            try
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load bill: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SetSplitMode(int tableNumber, SplitMode splitMode, int splitCount = 1)
        {
            try
            {
                TableOrderViewModel orders = _tableService.GetTableOrders(tableNumber);
                var viewModel = BuildBillViewModel(orders, splitMode, splitCount);
                AddPayers(viewModel, orders, splitMode);
                return View("Pay", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to set split mode: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private BillViewModel BuildBillViewModel(TableOrderViewModel orders, SplitMode splitMode, int splitCount)
        {
            return new BillViewModel
            {
                TableNumber    = orders.TableNumber,
                Guests         = orders.Guests,
                SubTotalAmount = orders.SubTotalAmount,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount,
                SplitMode      = splitMode,
                SplitCount     = splitMode == SplitMode.Equal ? Math.Max(1, splitCount) : 1
            };
        }

        private void AddPayers(BillViewModel viewModel, TableOrderViewModel orders, SplitMode splitMode)
        {
            if (splitMode == SplitMode.Equal)
                AddEqualPayers(viewModel);
            else if (splitMode == SplitMode.Custom)
                AddCustomPayers(viewModel);
            else if (splitMode == SplitMode.ByGuest)
                AddByGuestPayers(viewModel, orders);
        }

        private void AddEqualPayers(BillViewModel viewModel)
        {
            decimal share = Math.Round(viewModel.TotalAmount / viewModel.SplitCount, 2);
            for (int i = 1; i <= viewModel.SplitCount; i++)
            {
                viewModel.Payers.Add(new SplitPayerViewModel { Name = $"Person {i}", AmountDue = share });
            }
        }

        private void AddCustomPayers(BillViewModel viewModel)
        {
            viewModel.Payers.Add(new SplitPayerViewModel { Name = "Person 1", AmountDue = 0 });
        }

        private void AddByGuestPayers(BillViewModel viewModel, TableOrderViewModel orders)
        {
            foreach (var guest in orders.Guests)
            {
                decimal guestTotal = guest.Items.Sum(i => i.Price * i.Quantity);
                decimal vatShare = orders.TotalAmount > 0
                    ? guestTotal * (orders.TotalAmount / orders.SubTotalAmount)
                    : guestTotal;

                viewModel.Payers.Add(new SplitPayerViewModel
                {
                    Name      = guest.FullName,
                    GuestID   = guest.GuestID,
                    AmountDue = Math.Round(vatShare, 2)
                });
            }
        }

        [HttpPost]
        public IActionResult Pay(BillViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (model.SplitMode == SplitMode.Custom)
                {
                    decimal totalPaying = model.Payers.Sum(p => p.AmountDue + p.TipAmount);
                    if (totalPaying < model.TotalAmount)
                    {
                        ModelState.AddModelError("", $"Total payments (€{totalPaying:F2}) are less than the bill total (€{model.TotalAmount:F2}).");
                        var orders = _tableService.GetTableOrders(model.TableNumber);
                        model.Guests = orders.Guests;
                        return View(model);
                    }
                }

                _billService.ProcessPayment(model);
                return RedirectToAction("Confirmation", new { id = model.TableNumber });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Payment failed: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            ViewBag.TableNumber = id;
            return View();
        }
    }
}
