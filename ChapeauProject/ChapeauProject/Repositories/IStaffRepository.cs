using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface IStaffRepository
    {
        List<Staff> GetAllStaff();
        Staff? GetStaffById(int staffID);
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
