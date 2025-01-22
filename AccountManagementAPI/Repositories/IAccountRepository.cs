using DataAccess.Models;
using DataAccess.Repositories;

namespace AccountManagementAPI.Repositories
{
    public interface IAccountRepository : IRepository<User>
    {
        Task<IEnumerable<dynamic>> GetAllAccount();
        Task<User> GetUserByPhoneNumber(string phoneNumber);
        Task<User> GetUserByEmail(string email);
    }
}
