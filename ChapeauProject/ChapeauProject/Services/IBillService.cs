using ChapeauProject.ViewModels;

namespace ChapeauProject.Services
{
    public interface IBillService
    {
        
        void ProcessPayment(BillViewModel model);
    }
}
