using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface IBillService
    {
        BillViewModel GetPayViewModel(int tableNumber);
        BillViewModel GetSplitViewModel(int tableNumber, SplitMode splitMode, int splitCount);
        void ProcessPayment(BillViewModel model);
    }
}
