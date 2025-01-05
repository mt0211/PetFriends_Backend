using DataAccess.Models;
using DataAccess.Repositories;

namespace ClinicServiceManagement.Repository
{
    public interface IClinicServiceRepository : IRepository<ClinicService>
    {
        Task<IEnumerable<dynamic>> GetAllClinicService();
        Task<IEnumerable<Category>> GetAllCategory();
        Task<dynamic> GetServiceByID(Guid id);
        Task UpdateDiscountedPrice(ClinicService service);
    }
}
