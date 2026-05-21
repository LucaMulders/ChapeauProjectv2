using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface IStaffRepository
    {
        List<Staff> GetAll();
        Staff? GetById(int staffID);
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
