using ChapeauProject.Models;
using ChapeauProject.Repositories;

namespace ChapeauProject.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        public StaffService(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public List<Staff> GetAll()
        {
            return _staffRepository.GetAll();
        }

        public Staff? GetById(int id)
        {
            return _staffRepository.GetById(id);
        }

        public Staff? GetByLoginCredentials(int staffID, string password)
        {
            Staff? staff = _staffRepository.GetByLoginCredentials(staffID, password);
            if (staff == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, staff.Password) ? staff : null;
        }
    }
}