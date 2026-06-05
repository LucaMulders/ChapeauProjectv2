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

        //NOTE Split into seperate methods
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
                string? singleFeedback;
                if (string.IsNullOrWhiteSpace(model.Feedback))
                {
                    singleFeedback = null;
                }
                else
                {
                    singleFeedback = model.Feedback;
                }

                _billRepository.CreatePayment(new Payment
                {
                    BillID           = billId,
                    PaymentAmount    = model.TotalAmount + model.TipAmount,
                    PaymentMethod    = model.PaymentMethod,
                    TipAmount        = model.TipAmount,
                    PaymentTimeStamp = now,
                    Feedback         = singleFeedback
                });
            }
            else
            {
                foreach (var payer in model.Payers)
                {
                    string? payerFeedback;
                    if (string.IsNullOrWhiteSpace(payer.Feedback))
                    {
                        payerFeedback = null;
                    }
                    else
                    {
                        payerFeedback = payer.Feedback;
                    }

                    _billRepository.CreatePayment(new Payment
                    {
                        BillID           = billId,
                        PaymentAmount    = payer.AmountDue + payer.TipAmount,
                        PaymentMethod    = payer.PaymentMethod,
                        TipAmount        = payer.TipAmount,
                        PaymentTimeStamp = now,
                        Feedback         = payerFeedback
                    });
                }
            }

            _billRepository.CompleteOrdersForTable(model.TableNumber);
            _tableRepository.SetFree(model.TableNumber);
        }
    }
}
