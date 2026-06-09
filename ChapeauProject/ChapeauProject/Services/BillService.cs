using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _billRepository;
        private readonly ITableService   _tableService;

        public BillService(IBillRepository billRepository, ITableService tableService)
        {
            _billRepository = billRepository;
            _tableService   = tableService;
        }

        public BillViewModel GetPayViewModel(int tableNumber)
        {
            var orders = _tableService.GetTableOrders(tableNumber);
            return new BillViewModel
            {
                TableNumber    = orders.TableNumber,
                Guests         = orders.Guests,
                SubTotalAmount = orders.SubTotalAmount,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount
            };
        }

        public BillViewModel GetSplitViewModel(int tableNumber, SplitMode splitMode, int splitCount)
        {
            var orders    = _tableService.GetTableOrders(tableNumber);
            var viewModel = BuildBillViewModel(orders, splitMode, splitCount);
            AddPayers(viewModel, orders, splitMode);
            return viewModel;
        }

        public string? ValidatePayment(BillViewModel model)
        {
            if (model.SplitMode == SplitMode.Custom)
            {
                decimal totalPaying = model.Payers.Sum(p => p.AmountDue + p.TipAmount);
                if (totalPaying < model.TotalAmount)
                    return $"Total payments (€{totalPaying:F2}) are less than the bill total (€{model.TotalAmount:F2}).";
            }

            return null;
        }

        public void ProcessPayment(BillViewModel model)
        {
            var now    = DateTime.Now;
            int billId = CreateBill(model, now);

            if (model.SplitMode == SplitMode.Single)
                CreateSinglePayment(model, billId, now);
            else
                CreateSplitPayments(model, billId, now);

            _billRepository.CompleteOrdersForTable(model.TableNumber);
            _tableService.MarkTableAsFree(model.TableNumber);
            _tableService.RemoveGuests(model.TableNumber);
        }

        private BillViewModel BuildBillViewModel(TableOrderViewModel orders, SplitMode splitMode, int splitCount)
        {
            int resolvedSplitCount;
            if (splitMode == SplitMode.Equal)
                resolvedSplitCount = Math.Max(1, splitCount);
            else
                resolvedSplitCount = 1;

            return new BillViewModel
            {
                TableNumber    = orders.TableNumber,
                Guests         = orders.Guests,
                SubTotalAmount = orders.SubTotalAmount,
                LowVAT         = orders.LowVAT,
                HighVAT        = orders.HighVAT,
                TotalAmount    = orders.TotalAmount,
                SplitMode      = splitMode,
                SplitCount     = resolvedSplitCount
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
            decimal share     = Math.Round(viewModel.TotalAmount / viewModel.SplitCount, 2);
            decimal allocated = share * (viewModel.SplitCount - 1);
            decimal lastShare = viewModel.TotalAmount - allocated;

            for (int i = 1; i <= viewModel.SplitCount; i++)
            {
                decimal amount;
                if (i == viewModel.SplitCount)
                    amount = lastShare;
                else
                    amount = share;

                viewModel.Payers.Add(new SplitPayerViewModel { Name = $"Person {i}", AmountDue = amount });
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
                decimal vatShare;
                if (orders.TotalAmount > 0)
                    vatShare = guestTotal * (orders.TotalAmount / orders.SubTotalAmount);
                else
                    vatShare = guestTotal;

                viewModel.Payers.Add(new SplitPayerViewModel
                {
                    Name      = guest.FullName,
                    GuestID   = guest.GuestID,
                    AmountDue = Math.Round(vatShare, 2)
                });
            }
        }

        private int CreateBill(BillViewModel model, DateTime now)
        {
            var bill = new Bill
            {
                TotalAmount    = model.TotalAmount,
                VatAmount      = model.LowVAT + model.HighVAT,
                SubTotalAmount = model.SubTotalAmount,
                BillTimeStamp  = now
            };
            return _billRepository.CreateBill(bill);
        }

        private void CreateSinglePayment(BillViewModel model, int billId, DateTime now)
        {
            _billRepository.CreatePayment(new Payment
            {
                Bill             = new Bill { BillID = billId },
                PaymentAmount    = model.TotalAmount + model.TipAmount,
                PaymentMethod    = model.PaymentMethod,
                TipAmount        = model.TipAmount,
                PaymentTimeStamp = now,
                Feedback         = NullIfEmpty(model.Feedback)
            });
        }

        private void CreateSplitPayments(BillViewModel model, int billId, DateTime now)
        {
            foreach (var payer in model.Payers)
            {
                _billRepository.CreatePayment(new Payment
                {
                    Bill             = new Bill { BillID = billId },
                    PaymentAmount    = payer.AmountDue + payer.TipAmount,
                    PaymentMethod    = payer.PaymentMethod,
                    TipAmount        = payer.TipAmount,
                    PaymentTimeStamp = now,
                    Feedback         = NullIfEmpty(payer.Feedback)
                });
            }
        }

        private static string? NullIfEmpty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value;
        }
    }
}
