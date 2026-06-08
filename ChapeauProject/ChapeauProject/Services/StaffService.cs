using ChapeauProject.Models;
using ChapeauProject.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace ChapeauProject.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        public StaffService(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public List<Staff> GetAllStaff()
        {
            return _staffRepository.GetAllStaff();
        }

        public Staff? GetStaffById(int id)
        {
            return _staffRepository.GetStaffById(id);
        }

        public Staff? GetByLoginCredentials(int staffID, string password)
        {
            return _staffRepository.GetByLoginCredentials(staffID, HashPassword(password));
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}