using ChapeauProject.Models;

namespace ChapeauProject.Services
{
    public interface IStaffService
    {
        List<Staff> GetAllStaff();
        Staff? GetStaffById(int id);
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
