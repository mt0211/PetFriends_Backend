using DataAccess.Models;
using DataAccess.Repositories;

namespace AppUserAuthenticationAPI.Repositories
{
    public interface IAppUserAuthenticationRepository : IRepository<User>
    {

        public Task<User> GetUserByEmail(string Email);

        Task<User> GetUserByOTP(string otp, string email);
        Task<User> GetUserByPhoneNumber(string phoneNumber);
    }
}
