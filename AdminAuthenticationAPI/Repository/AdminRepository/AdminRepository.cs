using AdminAuthenticationAPI.DTOs.AdminDTOs;
using AdminAuthenticationAPI.DTOs.ResultModelAdmin;
using AdminAuthenticationAPI.Repository.AdminRepository;
//using AdminAuthenticationAPI.Repository.VerifyAdmminRepository;
using AdminAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;

namespace AdminAuthenticationAPI.Repository.AdminRepository
{
    public class AdminRepository : Repository<User>, IAdminRepository
    {
        private readonly PetfriendsContext _context;

        public AdminRepository(PetfriendsContext context) : base(context)
        {
            _context = context;
        }

       

        public async Task<User> GetAdminByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;
            }
            return user;
        }

       
    }
}
