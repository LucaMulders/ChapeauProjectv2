using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface IBillRepository
    {
        // Creates a Bill record and returns the generated BillID.
        int CreateBill(Bill bill);

        // Creates a Payment record linked to a Bill.
        void CreatePayment(Payment payment);

        // Marks all pending orders for a table as Complete.
        void CompleteOrdersForTable(int tableNumber);
    }
}
