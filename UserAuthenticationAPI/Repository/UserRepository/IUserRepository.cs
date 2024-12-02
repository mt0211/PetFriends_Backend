
using DataAccess.Models;
using DataAccess.Repositories;
namespace UserAuthenticationAPI.Repository.UserRepository
{
public interface IUserRepository : IRepository<User>
    {
        public Task<User> GetUserByEmail(string Email);

        Task<User> GetUserByOTP(string otp, string email);
        Task<User> GetUserByPhoneNumber(string phoneNumber);
    }
}