using ChapeauProject.Models;

namespace ChapeauProject.Services
{
    public interface IStaffService
    {
        List<Staff> GetAll();
        Staff? GetById(int id);
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
