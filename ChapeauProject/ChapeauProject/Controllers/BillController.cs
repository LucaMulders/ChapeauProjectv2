using ChapeauProject.Models;
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
                            GuestCount    = _tableService.GetGuestCount(t.TableNumber),
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
        public IActionResult Pay(Table table)
        {
            try
            {
                return View(_billService.GetPayViewModel(table.TableNumber));
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
                return View("Pay", _billService.GetSplitViewModel(tableNumber, splitMode, splitCount));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to set split mode: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Pay(BillViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                string? validationError = _billService.ValidatePayment(model);
                if (validationError != null)
                {
                    ModelState.AddModelError("", validationError);
                    model.Guests = _tableService.GetTableOrders(model.TableNumber).Guests;
                    return View(model);
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
