

using DataAccess.Models;
using DataAccess.Repositories;

namespace ProfileManagementAppAPI.Repositories
{
    public interface IAppointmentRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetCategory();
    }
}
