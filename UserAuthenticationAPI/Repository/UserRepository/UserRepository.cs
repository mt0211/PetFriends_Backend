using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UserAuthenticationAPI.Repository.UserRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly PetFriendsContext _context;

        public UserRepository(PetFriendsContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmail(string Email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user == null)
            {
               return null;
            }
            return user;
        }
        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<User> GetUserByOTP(string otp, string email)
        {
            var user = await GetUserByEmail(email);
            var otpverify = await _context.OtpVerifies
                .AsNoTracking()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OtpCode == otp && o.IsUsed == 0 && o.ExpiredAt > DateTime.UtcNow);
            if (otpverify == null)
            {
               return null;
            }

            if (user.Id != otpverify.User.Id)
            {
               return null;
            }
            return otpverify.User;
        }
    }
}