using ChapeauProject.Models;
using ChapeauProject.Repositories;
using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public class BillService : IBillService
    {
        private readonly IBillRepository  _billRepository;
        private readonly ITableRepository _tableRepository;

        public BillService(IBillRepository billRepository, ITableRepository tableRepository)
        {
            _billRepository  = billRepository;
            _tableRepository = tableRepository;
        }

        public void ProcessPayment(BillViewModel model)
        {
            var now   = DateTime.Now;
            int billId = CreateBill(model, now);

            if (model.SplitMode == SplitMode.Single)
                CreateSinglePayment(model, billId, now);
            else
                CreateSplitPayments(model, billId, now);

            _billRepository.CompleteOrdersForTable(model.TableNumber);
            _tableRepository.SetFree(model.TableNumber);
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

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
