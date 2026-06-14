using ChapeauProject.Models;

namespace ChapeauProject.Repositories
{
    public interface IStaffRepository
    {
        Staff? GetByLoginCredentials(int staffID, string password);
    }
}
