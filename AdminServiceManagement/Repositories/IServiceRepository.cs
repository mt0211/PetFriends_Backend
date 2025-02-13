using DataAccess.Models;
using DataAccess.Repositories;

namespace AdminServiceManagement.Repositories
{
    public interface IServiceRepository : IRepository<ClinicService>
    {
        Task<IEnumerable<dynamic>> GetAllService();
        Task<User> GetUserByID(Guid id);
        Task UpdateStatus(ClinicService service);
    }
}
