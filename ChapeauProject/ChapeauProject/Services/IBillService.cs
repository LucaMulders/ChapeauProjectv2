using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface IBillService
    {
        // Orchestrates: create Bill + Payment records, complete orders, free the table.
        void ProcessPayment(BillViewModel model);
    }
}
