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
            var now = DateTime.Now;

            var bill = new Bill
            {
                TotalAmount    = model.TotalAmount,
                VatAmount      = model.LowVAT + model.HighVAT,
                SubTotalAmount = model.SubTotalAmount,
                BillTimeStamp  = now
            };
            int billId = _billRepository.CreateBill(bill);

            if (model.SplitMode == SplitMode.Single)
            {
                _billRepository.CreatePayment(new Payment
                {
                    BillID           = billId,
                    PaymentAmount    = model.TotalAmount + model.TipAmount,
                    PaymentMethod    = model.PaymentMethod,
                    TipAmount        = model.TipAmount,
                    PaymentTimeStamp = now,
                    Feedback         = string.IsNullOrWhiteSpace(model.Feedback) ? null : model.Feedback
                });
            }
            else
            {
                foreach (var payer in model.Payers)
                {
                    _billRepository.CreatePayment(new Payment
                    {
                        BillID           = billId,
                        PaymentAmount    = payer.AmountDue + payer.TipAmount,
                        PaymentMethod    = payer.PaymentMethod,
                        TipAmount        = payer.TipAmount,
                        PaymentTimeStamp = now,
                        Feedback         = string.IsNullOrWhiteSpace(payer.Feedback) ? null : payer.Feedback
                    });
                }
            }

            _billRepository.CompleteOrdersForTable(model.TableNumber);
            _tableRepository.SetFree(model.TableNumber);
        }
    }
}
