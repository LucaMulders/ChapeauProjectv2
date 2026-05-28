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

            // 1. Create a Bill record for this table's session.
            var bill = new Bill
            {
                TotalAmount    = model.TotalAmount,
                VatAmount      = model.LowVAT + model.HighVAT,
                SubTotalAmount = model.SubTotalAmount,
                BillTimeStamp  = now
            };
            int billId = _billRepository.CreateBill(bill);

            // 2. Create a Payment record linked to the Bill.
            var payment = new Payment
            {
                BillID           = billId,
                PaymentAmount    = model.TotalAmount + model.TipAmount,
                PaymentMethod    = model.PaymentMethod,
                TipAmount        = model.TipAmount,
                PaymentTimeStamp = now,
                Feedback         = string.IsNullOrWhiteSpace(model.Feedback) ? null : model.Feedback
            };
            _billRepository.CreatePayment(payment);

            // 3. Mark all pending orders for this table as Complete.
            _billRepository.CompleteOrdersForTable(model.TableNumber);

            // 4. Set the table to free.
            _tableRepository.SetFree(model.TableNumber);
        }
    }
}
