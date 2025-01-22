using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AccountManagementAPI.Repositories
{
    public class AccountRepository : Repository<User>, IAccountRepository
    {
        private readonly PetfriendsContext _context;
        public AccountRepository(PetfriendsContext context):base(context)
        {
            _context = context;
        }
       public async Task<IEnumerable<dynamic>> GetAllAccount()
        {
            return await _context.Users.Select(c=> new
            {
                c.Id,
                c.Role,
                c.FullName,
                c.Email,
                c.PhoneNumber,
                c.AvatarUrl,
                c.Status,
            }).ToListAsync();
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

        public async Task<User> GetUserByEmail(string email)
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
