using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface IBillRepository
    {
        int CreateBill(Bill bill);
        void CreatePayment(Payment payment);
        void CompleteOrdersForTable(int tableNumber);
        int? GetBillIdForTable(int tableNumber);
        decimal GetAmountPaidForBill(int billId);
    }
}
