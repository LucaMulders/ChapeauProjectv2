using ChapeauProject.Models;

namespace ChapeauProject.Services
{
    public interface IStaffService
    {
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
