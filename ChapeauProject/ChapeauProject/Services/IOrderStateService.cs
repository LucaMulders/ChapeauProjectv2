using ChapeauProject.Models;

namespace ChapeauProject.Services
{
    public interface IOrderStateService
    {
        Order ActiveWorkingOrder { get; set; }
    }
}
