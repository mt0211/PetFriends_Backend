using DataAccess.Models;
using DataAccess.Repositories;

namespace ClinicServiceManagement.Repository
{
    public interface IClinicServiceRepository : IRepository<ClinicService>
    {
        Task<IEnumerable<dynamic>> GetAllClinicService();
        Task<IEnumerable<Category>> GetAllCategory();
        Task<dynamic> GetServiceByID(Guid id);
        Task<ClinicService> GetServiceStatusByID(Guid id);
        Task UpdateStatus(ClinicService service);
        Task UpdateService(ClinicService service);
    }
}
