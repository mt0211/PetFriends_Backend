using AdminAuthenticationAPI.DTOs.AdminDTOs;
using AdminAuthenticationAPI.DTOs.ResultModelAdmin;
using DataAccess.Models;
using DataAccess.Repositories;

namespace AdminAuthenticationAPI.Repository.AdminRepository
{
    public interface IAdminRepository : IRepository<User>
    {
        Task<User> GetAdminByEmail(string email);
    }
}
