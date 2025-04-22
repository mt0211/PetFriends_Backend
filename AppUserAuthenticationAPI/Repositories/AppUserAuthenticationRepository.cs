using AppUserAuthenticationAPI.Services;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AppUserAuthenticationAPI.Repositories
{
    public class AppUserAuthenticationRepository : Repository<User>, IAppUserAuthenticationRepository
    {

        private readonly PetfriendsContext _context;

        public AppUserAuthenticationRepository(PetfriendsContext context) : base(context)
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

        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<int> GetNumberOfAppointment(Guid userId)
        {
            var numberOfAppointment = await _context.Appointments.CountAsync(a => a.UserId == userId);
            return numberOfAppointment;
        }
        public async Task UpdateUserTypeGroup(User user)
        {
               _context.Users.Attach(user);
               _context.Entry(user).Property(u => u.TypeGroup).IsModified = true;
               await _context.SaveChangesAsync();
        }

        public async Task CreateChatMessage(ChatMessage chatMessage)
        {
            await _context.ChatMessages.AddAsync(chatMessage);
            await _context.SaveChangesAsync();
        }
    }
}
