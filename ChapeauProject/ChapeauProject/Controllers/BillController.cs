using ChapeauProject.Models;
using ChapeauProject.Services;
using ChapeauProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    [Authorize(Roles = "Waiter,Manager")]
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
                var tables = _tableService.GetTableSummaries(occupiedOnly: true);
                return View(tables);
            }
            catch (Exception ex)
            {
                // Changed errors to be more generic to avoid giving away information about the system

                Console.Error.WriteLine($"[BillController.Index] {ex}");
                TempData["ErrorMessage"] = "Failed to load bill overview. Please try again.";
                return View(new List<TablesViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Pay(int tableNumber)
        {
            try
            {
                return View(_billService.GetPayViewModel(tableNumber));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[BillController.Pay GET] {ex}");
                TempData["ErrorMessage"] = "Failed to load bill. Please try again.";
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
                Console.Error.WriteLine($"[BillController.SetSplitMode] {ex}");
                TempData["ErrorMessage"] = "Failed to set split mode. Please try again.";
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
                return RedirectToAction("Confirmation", new { tableNumber = model.TableNumber });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[BillController.Pay POST] {ex}");
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Confirmation(int tableNumber)
        {
            return View(tableNumber);
        }
    }
}
